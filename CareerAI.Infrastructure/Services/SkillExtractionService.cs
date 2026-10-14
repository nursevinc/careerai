using CareerAI.Application.DTOs.Ai;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;
using CareerAI.Application.Services;
using CareerAI.Domain.Entities;
using CareerAI.Domain.Enum;
using CareerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Infrastructure.Services;

public class SkillExtractionService(
    AppDbContext db,
    ISkillExtractor extractor,
    SkillCatalog skillCatalog,
    ILogger<SkillExtractionService> logger) : ISkillExtractionService
{
    public async Task<ResumeSkillsDto> ExtractResumeSkillsAsync(
     Guid userId, Guid resumeId, CancellationToken ct = default)
    {
        // Kullanıcı izolasyonu: başkasının CV'si için 404
        var resume = await db.Resumes
            .Include(r => r.Skills)
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId, ct)
            ?? throw new NotFoundException("Resume not found.");

        var extraction = await extractor.ExtractCvSkillsAsync(resume.ExtractedText, ct);

        // 1) Uydurma kontrolü: kanıtı CV metninde geçmeyen beceri atılır
        var grounded = SkillSanitizer.KeepGrounded(extraction.Skills, resume.ExtractedText, out var dropped);

        if (dropped > 0)
            logger.LogWarning(
                "Dropped {Dropped} AI-extracted skill(s) whose evidence was not found in the resume text (resume {ResumeId})",
                dropped, resumeId);

        // 2) Eşleştirmede kullanılacak standart adlar: temizle, tekrarları at
        var skills = SkillSanitizer.CleanSkills(grounded.Select(s => s.Name));

        var catalog = await skillCatalog.GetOrCreateAsync(skills, ct);

        // Fark yöntemi: sadece değişenleri sil/ekle (unique index çakışması olmaz, tek SaveChanges)
        var targetIds = skills.Select(name => catalog[name].Id).ToHashSet();
        var existingIds = resume.Skills.Select(rs => rs.SkillId).ToHashSet();

        db.ResumeSkills.RemoveRange(resume.Skills.Where(rs => !targetIds.Contains(rs.SkillId)).ToList());

        foreach (var skillId in targetIds.Where(id => !existingIds.Contains(id)))
            db.ResumeSkills.Add(new ResumeSkill { ResumeId = resume.Id, SkillId = skillId });

        await SaveAsync(ct);

        logger.LogInformation("Resume skills saved: {Count} skill(s) (resume {ResumeId}, user {UserId})",
            skills.Count, resumeId, userId);

        return new ResumeSkillsDto(resume.Id, skills.Select(name => catalog[name].Name).Distinct().ToList());
    }

    public async Task<JobRequirementsDto> ExtractJobRequirementsAsync(
        Guid userId, Guid jobApplicationId, CancellationToken ct = default)
    {
        var application = await db.JobApplications
            .Include(a => a.Skills)
            .FirstOrDefaultAsync(a => a.Id == jobApplicationId && a.UserId == userId, ct)
            ?? throw new NotFoundException("Job application not found.");

        var extraction = await extractor.ExtractJobRequirementsAsync(
            application.JobTitle, application.JobDescription, ct);

        var required = SkillSanitizer.CleanSkills(extraction.RequiredSkills);
        var preferred = SkillSanitizer.RemoveOverlap(SkillSanitizer.CleanSkills(extraction.PreferredSkills), required);
        var experienceLevel = SkillSanitizer.NormalizeExperienceLevel(extraction.ExperienceLevel);
        var otherRequirements = SkillSanitizer.CleanList(
            extraction.OtherRequirements,
            SkillSanitizer.MaxOtherRequirements,
            SkillSanitizer.MaxOtherRequirementLength);

        var catalog = await skillCatalog.GetOrCreateAsync(required.Concat(preferred).ToList(), ct);

        // Hedef durum: beceri -> zorunlu mu, tercih mi?
        var targets = new Dictionary<Guid, SkillRequirementType>();
        foreach (var name in required)
            targets[catalog[name].Id] = SkillRequirementType.Required;
        foreach (var name in preferred)
            targets.TryAdd(catalog[name].Id, SkillRequirementType.Preferred);

        db.JobApplicationSkills.RemoveRange(
            application.Skills.Where(js => !targets.ContainsKey(js.SkillId)).ToList());

        foreach (var (skillId, requirementType) in targets)
        {
            var existing = application.Skills.FirstOrDefault(js => js.SkillId == skillId);
            if (existing is null)
            {
                db.JobApplicationSkills.Add(new JobApplicationSkill
                {
                    JobApplicationId = application.Id,
                    SkillId = skillId,
                    RequirementType = requirementType,
                    IsMatched = false // eşleşme Aşama 7'de hesaplanacak
                });
            }
            else
            {
                existing.RequirementType = requirementType;
                existing.IsMatched = false;
            }
        }

        await SaveAsync(ct);

        logger.LogInformation(
            "Job requirements saved: {Required} required, {Preferred} preferred (application {ApplicationId}, user {UserId})",
            required.Count, preferred.Count, jobApplicationId, userId);

        return new JobRequirementsDto(
            application.Id,
            required.Select(name => catalog[name].Name).ToList(),
            preferred.Select(name => catalog[name].Name).ToList(),
            experienceLevel,
            otherRequirements);
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Aynı kayıt için iki çıkarma isteği aynı anda çalıştı
            throw new ConflictException("The same operation is already in progress. Please try again.");
        }
    }
}