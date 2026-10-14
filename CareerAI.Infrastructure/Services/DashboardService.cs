using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Dashboard;
using CareerAI.Application.Interfaces;
using CareerAI.Application.Services;
using CareerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerAI.Infrastructure.Services;

public class DashboardService(AppDbContext db) : IDashboardService
{
    private const int MonthsInSeries = 6;
    private const int TopMissingSkillCount = 8;
    private const int RecentAnalysisCount = 5;

    public async Task<DashboardDto> GetAsync(Guid userId, CancellationToken ct = default)
    {
        // Her sorgu UserId filtresiyle başlar: kullanıcı izolasyonu
        var applications = db.JobApplications.AsNoTracking().Where(a => a.UserId == userId);

        // "Analiz edilmiş" = güncel skoru olan ilan (yeniden analiz aynı ilanı tekrar saydırmaz)
        var analyzed = applications.Where(a => a.MatchScore != null);

        // 1) Özet sayılar: tek sorguda COUNT / AVG / MAX (satırlar uygulamaya hiç gelmez)
        var totals = await applications
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Analyzed = g.Count(a => a.MatchScore != null),
                Average = g.Average(a => a.MatchScore), // NULL'ları yok sayar
                Highest = g.Max(a => a.MatchScore)
            })
            .FirstOrDefaultAsync(ct); // hiç ilan yoksa null

        var stats = new DashboardStatsDto(
            totals?.Total ?? 0,
            totals?.Analyzed ?? 0,
            totals?.Average is { } average ? (int)Math.Round(average, MidpointRounding.AwayFromZero) : null,
            totals?.Highest);

        // 2) Skor dağılımı: skor / 20 tamsayı bölmesiyle SQL'de gruplanır (en fazla 6 satır döner)
        var buckets = await analyzed
            .GroupBy(a => (a.MatchScore ?? 0) / DashboardCalculator.ScoreBucketSize)
            .Select(g => new { Bucket = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var distribution = DashboardCalculator.BuildScoreDistribution(
            buckets.Select(b => (b.Bucket, b.Count)));

        // 3) Zamana göre başvurular: son 6 ayın tarihlerini çek, ay bazında bellekte grupla
        var now = DateTime.UtcNow;
        var seriesStart = DashboardCalculator.SeriesStart(now, MonthsInSeries);

        var createdDates = await applications
            .Where(a => a.CreatedAt >= seriesStart)
            .Select(a => a.CreatedAt)
            .ToListAsync(ct);

        var monthlyCounts = createdDates
            .GroupBy(date => (date.Year, date.Month))
            .Select(g => (g.Key.Year, g.Key.Month, Count: g.Count()));

        var overTime = DashboardCalculator.BuildMonthlySeries(monthlyCounts, now, MonthsInSeries);

        // 4) En sık eksik beceriler. MatchScore != null ŞART: analiz edilmemiş ilanlarda IsMatched varsayılan olarak false
        var missing = await db.JobApplicationSkills
            .AsNoTracking()
            .Where(js => !js.IsMatched
                         && js.JobApplication.UserId == userId
                         && js.JobApplication.MatchScore != null)
            .GroupBy(js => js.Skill.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name)
            .Take(TopMissingSkillCount)
            .ToListAsync(ct);

        // 5) Son analizler: her ilan bir kez, en son analiz tarihine göre
        var recent = await analyzed
            .OrderByDescending(a => a.Analyses.Max(x => (DateTime?)x.CreatedAt) ?? a.CreatedAt)
            .Take(RecentAnalysisCount)
            .Select(a => new
            {
                a.Id,
                a.CompanyName,
                a.JobTitle,
                Score = a.MatchScore ?? 0,
                AnalyzedAt = a.Analyses.Max(x => (DateTime?)x.CreatedAt) ?? a.CreatedAt
            })
            .ToListAsync(ct);

        return new DashboardDto(
            stats,
            distribution,
            overTime,
            missing.Select(m => new MissingSkillDto(m.Name, m.Count)).ToList(),
            recent.Select(r => new RecentAnalysisDto(r.Id, r.CompanyName, r.JobTitle, r.Score, r.AnalyzedAt)).ToList());
    }
}