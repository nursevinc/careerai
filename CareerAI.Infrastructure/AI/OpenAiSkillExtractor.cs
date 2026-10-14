using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using CareerAI.Application.DTOs.Ai;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.ClientModel;
namespace CareerAI.Infrastructure.AI;

// Modelin JSON şemasına uygun döndürdüğü yanıtların C# karşılıkları
internal sealed record CvSkillItem(string? Name, string? Evidence);

internal sealed record CvSkillsPayload(List<CvSkillItem>? Skills);

internal sealed record JobRequirementsPayload(
    List<string>? RequiredSkills,
    List<string>? PreferredSkills,
    string? ExperienceLevel,
    List<string>? OtherRequirements);

public class OpenAiSkillExtractor(OpenAiJsonClient client) : ISkillExtractor
{
    // Metnin içinden prompt etiketlerini kapatıp "talimat alanına" çıkmaya çalışmayı engeller
    private static readonly Regex PromptTags =
        new(@"</?\s*(resume|job_posting)\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<CvSkillExtraction> ExtractCvSkillsAsync(string resumeText, CancellationToken ct = default)
    {
        var userMessage = $"<resume>\n{Neutralize(resumeText)}\n</resume>";

        var payload = await client.CompleteAsync<CvSkillsPayload>(
            "cv_skills", Prompts.CvSkillsSystem, userMessage, Prompts.CvSkillsSchema, ct);

        var skills = (payload.Skills ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Evidence))
            .Select(item => new ExtractedSkill(item.Name!, item.Evidence!))
            .ToList();

        return new CvSkillExtraction(skills);
    }

    public async Task<JobRequirementExtraction> ExtractJobRequirementsAsync(
        string jobTitle, string jobDescription, CancellationToken ct = default)
    {
        var userMessage =
            $"<job_posting>\nTitle: {Neutralize(jobTitle)}\n\n{Neutralize(jobDescription)}\n</job_posting>";

        var payload = await client.CompleteAsync<JobRequirementsPayload>(
            "job_requirements", Prompts.JobRequirementsSystem, userMessage, Prompts.JobRequirementsSchema, ct);

        return new JobRequirementExtraction(
            payload.RequiredSkills ?? [],
            payload.PreferredSkills ?? [],
            payload.ExperienceLevel ?? "Unspecified",
            payload.OtherRequirements ?? []);
    }

    private static string Neutralize(string text) => PromptTags.Replace(text, string.Empty);
}