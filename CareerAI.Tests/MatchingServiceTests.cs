using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Services;
using Xunit;

namespace CareerAI.Tests;

public class MatchingServiceTests
{
    private readonly MatchingService _matching = new();

    [Fact]
    public void FiveRequired_FourMatched_ScoresEighty()
    {
        var result = _matching.CalculateMatch(
            ["C#", ".NET", "React", "SQL", "Git"],
            ["C#", ".NET", "React", "SQL", "Docker"],
            []);

        Assert.Equal(80, result.Score);
        Assert.Equal(new[] { "C#", ".NET", "React", "SQL" }, result.MatchedRequired);
        Assert.Equal(new[] { "Docker" }, result.MissingRequired);
    }

    [Fact]
    public void FiveRequired_AllMatched_ScoresHundred()
    {
        var result = _matching.CalculateMatch(
            ["C#", ".NET", "React", "SQL", "Docker"],
            ["C#", ".NET", "React", "SQL", "Docker"],
            []);

        Assert.Equal(100, result.Score);
        Assert.Empty(result.MissingRequired);
    }

    [Fact]
    public void FiveRequired_NoneMatched_ScoresZero()
    {
        var result = _matching.CalculateMatch(
            ["Git"],
            ["C#", ".NET", "React", "SQL", "Docker"],
            []);

        Assert.Equal(0, result.Score);
        Assert.Equal(5, result.MissingRequired.Count);
    }

    [Fact]
    public void RequiredMatched_PreferredMissing_ScoresSeventyFive()
    {
        // ağırlık: 3 / (3 + 1)
        var result = _matching.CalculateMatch(["C#"], ["C#"], ["Azure"]);

        Assert.Equal(75, result.Score);
        Assert.Equal(new[] { "Azure" }, result.MissingPreferred);
    }

    [Fact]
    public void RequiredMissing_PreferredMatched_ScoresTwentyFive()
    {
        // ağırlık: 1 / (3 + 1)
        var result = _matching.CalculateMatch(["Azure"], ["C#"], ["Azure"]);

        Assert.Equal(25, result.Score);
    }

    [Fact]
    public void WorkedExample_FourOfFiveRequired_OneOfTwoPreferred_ScoresSeventySix()
    {
        // (4*3 + 1*1) / (5*3 + 2*1) = 13 / 17 = 76,47
        var result = _matching.CalculateMatch(
            ["C#", "ASP.NET Core", "SQL Server", "React", "Azure"],
            ["C#", "ASP.NET Core", "SQL Server", "React", "Docker"],
            ["Azure", "Unit Testing"]);

        Assert.Equal(76, result.Score);
    }

    [Fact]
    public void Aliases_AreTreatedAsTheSameSkill()
    {
        var result = _matching.CalculateMatch(
            ["EF Core", "MSSQL", "ReactJS", "csharp"],
            ["Entity Framework", "SQL Server", "React", "C#"],
            []);

        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void VersionNumbersAndCasing_AreIgnored()
    {
        var result = _matching.CalculateMatch(["asp.net core 8"], ["ASP.NET Core"], []);

        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void ParentheticalAndTurkishNames_AreNormalized()
    {
        var result = _matching.CalculateMatch(["Nesne Tabanlı Programlama (OOP)"], ["OOP"], []);

        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Implication_SqlServerSatisfiesSql()
    {
        var result = _matching.CalculateMatch(["SQL Server"], ["SQL"], []);

        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Implication_IsOneDirectional_SqlDoesNotSatisfySqlServer()
    {
        var result = _matching.CalculateMatch(["SQL"], ["SQL Server"], []);

        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void NoJobSkills_ScoresZero()
    {
        var result = _matching.CalculateMatch(["C#"], [], []);

        Assert.Equal(0, result.Score);
        Assert.Empty(result.MatchedRequired);
        Assert.Empty(result.MissingRequired);
    }

    [Fact]
    public void DuplicateJobSkills_AreCountedOnce()
    {
        var result = _matching.CalculateMatch([], ["C#", "csharp", "Docker"], []);

        Assert.Equal(2, result.MissingRequired.Count);
    }

    [Fact]
    public void SkillListedAsRequiredAndPreferred_CountsOnlyAsRequired()
    {
        var result = _matching.CalculateMatch(["C#"], ["C#"], ["c#", "Azure"]);

        // toplam 3 (C#) + 1 (Azure) = 4, eşleşen 3
        Assert.Equal(75, result.Score);
        Assert.Empty(result.MatchedPreferred);
        Assert.Equal(new[] { "Azure" }, result.MissingPreferred);
    }
}