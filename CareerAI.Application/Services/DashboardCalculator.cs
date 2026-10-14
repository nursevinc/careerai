using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Dashboard;

namespace CareerAI.Application.Services;

// Veritabanından gelen toplam sonuçlarını grafiğe uygun hâle getiren saf mantık
public static class DashboardCalculator
{
    public const int ScoreBucketSize = 20;

    private static readonly string[] BucketLabels = ["0-19", "20-39", "40-59", "60-79", "80-100"];

    public static IReadOnlyList<ScoreBucketDto> BuildScoreDistribution(IEnumerable<(int Bucket, int Count)> grouped)
    {
        var counts = new int[BucketLabels.Length];

        foreach (var (bucket, count) in grouped)
            counts[Math.Clamp(bucket, 0, BucketLabels.Length - 1)] += count;

        return BucketLabels.Select((label, index) => new ScoreBucketDto(label, counts[index])).ToList();
    }

    // Son "months" ayı, en eskiden en yeniye. Verisi olmayan ay 0 olarak yer alır.
    public static IReadOnlyList<MonthlyCountDto> BuildMonthlySeries(
        IEnumerable<(int Year, int Month, int Count)> grouped, DateTime nowUtc, int months)
    {
        var lookup = grouped
            .GroupBy(item => (item.Year, item.Month))
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Count));

        var currentMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1);
        var series = new List<MonthlyCountDto>();

        for (var offset = months - 1; offset >= 0; offset--)
        {
            var month = currentMonth.AddMonths(-offset);
            lookup.TryGetValue((month.Year, month.Month), out var count);
            series.Add(new MonthlyCountDto($"{month.Year:D4}-{month.Month:D2}", count));
        }

        return series;
    }

    // Serinin ilk ayının ilk günü (sorgudaki alt sınır)
    public static DateTime SeriesStart(DateTime nowUtc, int months) =>
        new DateTime(nowUtc.Year, nowUtc.Month, 1).AddMonths(-(months - 1));
}