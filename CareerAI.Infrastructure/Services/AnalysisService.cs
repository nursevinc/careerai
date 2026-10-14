using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json;
using CareerAI.Application.DTOs.Analysis;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;
using CareerAI.Application.Services;
using CareerAI.Domain.Entities;
using CareerAI.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareerAI.Infrastructure.Services;

// AIAnalysis.AnalysisResult kolonuna JSON olarak yazılan biçim. Version: ileride şema değişirse eski kayıtları ayırt etmek için.
internal sealed record StoredSkill(string Name, bool IsRequired);

internal sealed record StoredAnalysis(
    int Version,
    int MatchScore,
    List<StoredSkill> MatchedSkills,
    List<StoredSkill> MissingSkills,
    string ExperienceLevel,
    List<string> OtherRequirements,
    string? Summary,
    List<string> Recommendations);

public class AnalysisService(
    AppDbContext db,
    ISkillExtractionService skillExtractionService,
    IMatchingService matchingService,
    IRecommendationGenerator recommendationGenerator,
    IValidator<AnalyzeRequest> validator,
    ILogger<AnalysisService> logger) : IAnalysisService
{
    private const int StoredAnalysisVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AnalysisResultDto> AnalyzeAsync(
        Guid userId, Guid jobApplicationId, AnalyzeRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        // 1) Yetki: ilan ve CV bu kullanıcıya ait mi? Değilse 404, AI'a hiç gitmeden.
        var application = await db.JobApplications
            .FirstOrDefaultAsync(a => a.Id == jobApplicationId && a.UserId == userId, ct)
            ?? throw new NotFoundException("Job application not found.");

        var resume = await db.Resumes
            .Include(r => r.Skills).ThenInclude(rs => rs.Skill)
            .FirstOrDefaultAsync(r => r.Id == request.ResumeId && r.UserId == userId, ct)
            ?? throw new NotFoundException("Resume not found.");

        // 2) CV becerileri: veritabanında varsa kullan, yoksa AI ile çıkar (önbellek)
        IReadOnlyList<string> candidateSkills = resume.Skills.Select(rs => rs.Skill.Name).ToList();
        if (candidateSkills.Count == 0)
            candidateSkills = (await skillExtractionService.ExtractResumeSkillsAsync(userId, resume.Id, ct)).Skills;

        if (candidateSkills.Count == 0)
            throw new BusinessRuleException("resume_no_skills", "No technical skills could be found in the resume.");

        // 3) İlan gereksinimleri (her analizde taze: ilan metni değişmiş olabilir)
        var requirements = await skillExtractionService.ExtractJobRequirementsAsync(userId, application.Id, ct);

        if (requirements.RequiredSkills.Count + requirements.PreferredSkills.Count == 0)
            throw new BusinessRuleException("job_no_skills", "No technical skills could be found in the job posting.");

        // 4) Deterministik eşleştirme ve skor (AI yok)
        var match = matchingService.CalculateMatch(
            candidateSkills, requirements.RequiredSkills, requirements.PreferredSkills);

        // 5) AI önerileri. Başarısız olursa analiz skorla birlikte yine kaydedilir.
        string? summary = null;
        IReadOnlyList<string> recommendations = [];

        try
        {
            var generated = await recommendationGenerator.GenerateAsync(
                new RecommendationRequest(
                    application.JobTitle,
                    requirements.ExperienceLevel,
                    match.Score,
                    candidateSkills,
                    match.MatchedRequired.Concat(match.MatchedPreferred).ToList(),
                    match.MissingRequired,
                    match.MissingPreferred,
                    requirements.OtherRequirements),
                ct);

            // AI çıktısına yine güvenmiyoruz: temizle ve sınırla
            summary = SkillSanitizer.CleanList([generated.Summary], 1, 500).FirstOrDefault();
            recommendations = SkillSanitizer.CleanList(generated.Recommendations, 5, 500);
        }
        catch (AiServiceException ex)
        {
            logger.LogWarning(
                "Recommendations could not be generated ({Code}); saving the analysis without them (application {ApplicationId})",
                ex.Code, application.Id);
        }

        // 6) Kaydet: ilandaki eşleşme bilgisi, genel skor ve analiz kaydı
        var matchedKeys = match.MatchedRequired.Concat(match.MatchedPreferred)
            .Select(SkillNormalizer.Normalize)
            .ToHashSet();

        var jobSkills = await db.JobApplicationSkills
            .Include(js => js.Skill)
            .Where(js => js.JobApplicationId == application.Id)
            .ToListAsync(ct);

        foreach (var jobSkill in jobSkills)
            jobSkill.IsMatched = matchedKeys.Contains(SkillNormalizer.Normalize(jobSkill.Skill.Name));

        application.MatchScore = match.Score;

        var stored = new StoredAnalysis(
            StoredAnalysisVersion,
            match.Score,
            ToStoredSkills(match.MatchedRequired, match.MatchedPreferred),
            ToStoredSkills(match.MissingRequired, match.MissingPreferred),
            requirements.ExperienceLevel,
            requirements.OtherRequirements.ToList(),
            summary,
            recommendations.ToList());

        var analysis = new AIAnalysis
        {
            JobApplicationId = application.Id,
            ResumeId = resume.Id,
            AnalysisResult = JsonSerializer.Serialize(stored, JsonOptions)
        };

        db.AIAnalyses.Add(analysis);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Analysis completed: score {Score}, {Matched} matched, {Missing} missing (application {ApplicationId}, user {UserId})",
            match.Score,
            stored.MatchedSkills.Count,
            stored.MissingSkills.Count,
            application.Id,
            userId);

        return ToDto(
            analysis.Id, analysis.CreatedAt, application.Id, resume.Id,
            application.JobTitle, application.CompanyName, resume.FileName, stored);
    }

    public async Task<AnalysisResultDto> GetLatestAsync(
        Guid userId, Guid jobApplicationId, CancellationToken ct = default)
    {
        // Projeksiyon: CV'nin büyük metnini çekmeden sadece gereken alanları al
        var row = await db.AIAnalyses
            .AsNoTracking()
            .Where(a => a.JobApplicationId == jobApplicationId && a.JobApplication.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.CreatedAt,
                a.JobApplicationId,
                a.ResumeId,
                a.AnalysisResult,
                a.JobApplication.JobTitle,
                a.JobApplication.CompanyName,
                ResumeFileName = a.Resume.FileName
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("No analysis found for this job application.");

        var stored = Deserialize(row.AnalysisResult, row.Id);

        return ToDto(
            row.Id, row.CreatedAt, row.JobApplicationId, row.ResumeId,
            row.JobTitle, row.CompanyName, row.ResumeFileName, stored);
    }

    private StoredAnalysis Deserialize(string json, Guid analysisId)
    {
        try
        {
            return JsonSerializer.Deserialize<StoredAnalysis>(json, JsonOptions)
                   ?? throw new JsonException("Stored analysis was null.");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Stored analysis {AnalysisId} could not be read", analysisId);
            throw new InvalidOperationException("The stored analysis is corrupted.", ex);
        }
    }

    private static List<StoredSkill> ToStoredSkills(IEnumerable<string> required, IEnumerable<string> preferred) =>
        required.Select(name => new StoredSkill(name, true))
            .Concat(preferred.Select(name => new StoredSkill(name, false)))
            .ToList();

    private static AnalysisResultDto ToDto(
        Guid id,
        DateTime createdAt,
        Guid jobApplicationId,
        Guid resumeId,
        string jobTitle,
        string companyName,
        string resumeFileName,
        StoredAnalysis stored) =>
        new(
            id,
            jobApplicationId,
            resumeId,
            jobTitle,
            companyName,
            resumeFileName,
            stored.MatchScore,
            stored.MatchedSkills.Select(s => new SkillMatchDto(s.Name, s.IsRequired)).ToList(),
            stored.MissingSkills.Select(s => new SkillMatchDto(s.Name, s.IsRequired)).ToList(),
            stored.ExperienceLevel,
            stored.OtherRequirements,
            stored.Summary,
            stored.Recommendations,
            createdAt);
}