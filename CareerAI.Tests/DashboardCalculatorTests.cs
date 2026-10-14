using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Services;
using Xunit;

namespace CareerAI.Tests;

public class DashboardCalculatorTests
{
    [Fact]
    public void ScoreDistribution_AlwaysReturnsFiveBuckets_EmptyOnesAreZero()
    {
        var result = DashboardCalculator.BuildScoreDistribution([(1, 3)]);

        Assert.Equal(5, result.Count);
        Assert.Equal("20-39", result[1].Label);
        Assert.Equal(3, result[1].Count);
        Assert.Equal(0, result[0].Count);
        Assert.Equal(0, result[4].Count);
    }

    [Fact]
    public void ScoreDistribution_ScoreHundredFallsIntoLastBucket()
    {
        // skor 100 / 20 = 5: son kovaya (80-100) eklenir
        var result = DashboardCalculator.BuildScoreDistribution([(5, 2), (4, 1)]);

        Assert.Equal("80-100", result[4].Label);
        Assert.Equal(3, result[4].Count);
    }

    [Fact]
    public void MonthlySeries_ReturnsRequestedMonthsOldestFirst_AndFillsEmptyMonthsWithZero()
    {
        var now = new DateTime(2026, 10, 13, 0, 0, 0, DateTimeKind.Utc);

        var result = DashboardCalculator.BuildMonthlySeries([(2026, 10, 4), (2026, 8, 1)], now, 3);

        Assert.Equal(["2026-08", "2026-09", "2026-10"], result.Select(m => m.Month).ToArray());
        Assert.Equal([1, 0, 4], result.Select(m => m.Count).ToArray());
    }

    [Fact]
    public void MonthlySeries_CrossesYearBoundary()
    {
        var now = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);

        var result = DashboardCalculator.BuildMonthlySeries([], now, 4);

        Assert.Equal(["2025-11", "2025-12", "2026-01", "2026-02"], result.Select(m => m.Month).ToArray());
        Assert.All(result, m => Assert.Equal(0, m.Count));
    }

    [Fact]
    public void SeriesStart_IsFirstDayOfTheOldestMonth()
    {
        var now = new DateTime(2026, 10, 13, 8, 30, 0, DateTimeKind.Utc);

        Assert.Equal(new DateTime(2026, 5, 1), DashboardCalculator.SeriesStart(now, 6));
    }
}