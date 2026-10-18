using System;
using System.Collections.Generic;
using System.Text;
using CareerAI.Application.DTOs.Ai;
using CareerAI.Application.DTOs.Analysis;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;

namespace CareerAI.Tests.Integration;

// OpenAI yerine geçen sahte sağlayıcı: deterministik, ücretsiz, çevrimdışı. Çağrı sayısını da tutar.
public sealed class FakeSkillExtractor : ISkillExtractor
{
    // Test metinlerinde geçen "bilinen" beceriler. (Test metinlerinde "git" içeren başka kelime kullanılmamalı.)
    private static readonly string[] KnownSkills =
        ["C#", "ASP.NET Core", "SQL Server", "React", "Docker", "Azure", "Git", "TypeScript"];

    private int _cvCalls;
    private int _jobCalls;

    public int CvCalls => Volatile.Read(ref _cvCalls);
    public int JobCalls => Volatile.Read(ref _jobCalls);

    public Task<CvSkillExtraction> ExtractCvSkillsAsync(string resumeText, CancellationToken ct = default)
    {
        Interlocked.Increment(ref _cvCalls);

        var skills = FindSkills(resumeText).Select(name => new ExtractedSkill(name, name)).ToList();
        return Task.FromResult(new CvSkillExtraction(skills));
    }

    public Task<JobRequirementExtraction> ExtractJobRequirementsAsync(
        string jobTitle, string jobDescription, CancellationToken ct = default)
    {
        Interlocked.Increment(ref _jobCalls);

        // Test ilanı biçimi: "Required: A, B. Preferred: C."
        var parts = jobDescription.Split("Preferred:", 2, StringSplitOptions.None);
        var required = FindSkills(parts[0]);
        var preferred = parts.Length > 1 ? FindSkills(parts[1]) : new List<string>();

        return Task.FromResult(new JobRequirementExtraction(required, preferred, "Junior", new List<string>()));
    }

    private static List<string> FindSkills(string text)
    {
        var haystack = Compact(text);
        return KnownSkills.Where(skill => haystack.Contains(Compact(skill), StringComparison.Ordinal)).ToList();
    }

    private static string Compact(string value) =>
        new(value.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || c is '#' or '+' or '.').ToArray());
}

public sealed class FakeRecommendationGenerator : IRecommendationGenerator
{
    // true ise AI servisi çökmüş gibi davranır
    public bool ShouldFail { get; set; }

    // AI'a giden son istek: içinde CV/ilan metni olmadığını doğrulamak için
    public RecommendationRequest? LastRequest { get; private set; }

    public Task<RecommendationResult> GenerateAsync(RecommendationRequest request, CancellationToken ct = default)
    {
        if (ShouldFail)
            throw new AiServiceException("ai_unavailable", "Simulated AI outage.");

        LastRequest = request;

        return Task.FromResult(new RecommendationResult(
            "Test summary.",
            ["Test recommendation one.", "Test recommendation two."]));
    }
}