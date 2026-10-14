using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Application.DTOs.Analysis;

public record AnalyzeRequest(Guid ResumeId);

// Eşleştirme algoritmasının çıktısı 
public record MatchResult(
    int Score,
    IReadOnlyList<string> MatchedRequired,
    IReadOnlyList<string> MissingRequired,
    IReadOnlyList<string> MatchedPreferred,
    IReadOnlyList<string> MissingPreferred);

public record SkillMatchDto(string Name, bool IsRequired);

public record AnalysisResultDto(
    Guid Id,
    Guid JobApplicationId,
    Guid ResumeId,
    string JobTitle,
    string CompanyName,
    string ResumeFileName,
    int MatchScore,
    IReadOnlyList<SkillMatchDto> MatchedSkills,
    IReadOnlyList<SkillMatchDto> MissingSkills,
    string ExperienceLevel,
    IReadOnlyList<string> OtherRequirements,
    string? Summary,
    IReadOnlyList<string> Recommendations,
    DateTime CreatedAt);

// AI'a giden öneri isteği. CV/ilan metni yok, sadece eşleştirme sonucu
public record RecommendationRequest(
    string JobTitle,
    string ExperienceLevel,
    int MatchScore,
    IReadOnlyList<string> CandidateSkills,
    IReadOnlyList<string> MatchedSkills,
    IReadOnlyList<string> MissingRequiredSkills,
    IReadOnlyList<string> MissingPreferredSkills,
    IReadOnlyList<string> OtherRequirements);

public record RecommendationResult(string Summary, IReadOnlyList<string> Recommendations);