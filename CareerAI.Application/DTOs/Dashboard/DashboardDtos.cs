using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Application.DTOs.Dashboard;

// TotalAnalyses: en az bir kez analiz edilmiş ilan sayısı (yeniden analiz sayıyı artırmaz)
public record DashboardStatsDto(int TotalApplications, int TotalAnalyses, int? AverageScore, int? HighestScore);

public record ScoreBucketDto(string Label, int Count);

// Month: "2026-10" biçiminde (yıl-ay)
public record MonthlyCountDto(string Month, int Count);

public record MissingSkillDto(string Name, int Count);

public record RecentAnalysisDto(
    Guid JobApplicationId,
    string CompanyName,
    string JobTitle,
    int MatchScore,
    DateTime AnalyzedAt);

public record DashboardDto(
    DashboardStatsDto Stats,
    IReadOnlyList<ScoreBucketDto> ScoreDistribution,
    IReadOnlyList<MonthlyCountDto> ApplicationsOverTime,
    IReadOnlyList<MissingSkillDto> TopMissingSkills,
    IReadOnlyList<RecentAnalysisDto> RecentAnalyses);