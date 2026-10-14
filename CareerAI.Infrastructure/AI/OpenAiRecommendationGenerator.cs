using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json;
using CareerAI.Application.DTOs.Analysis;
using CareerAI.Application.Interfaces;

namespace CareerAI.Infrastructure.AI;

internal sealed record RecommendationPayload(string? Summary, List<string>? Recommendations);

public class OpenAiRecommendationGenerator(OpenAiJsonClient client) : IRecommendationGenerator
{
    private static readonly JsonSerializerOptions DataJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<RecommendationResult> GenerateAsync(RecommendationRequest request, CancellationToken ct = default)
    {
        // Veriyi JSON olarak veriyoruz: serileştirici '<' '>' gibi karakterleri kaçırır,
        // yani bir beceri adı <analysis_data> etiketini kapatıp prompt'tan "kaçamaz".
        var data = JsonSerializer.Serialize(
            new
            {
                jobTitle = request.JobTitle,
                experienceLevel = request.ExperienceLevel,
                matchScore = request.MatchScore,
                candidateSkills = request.CandidateSkills,
                matchedSkills = request.MatchedSkills,
                missingRequiredSkills = request.MissingRequiredSkills,
                missingPreferredSkills = request.MissingPreferredSkills,
                otherRequirements = request.OtherRequirements
            },
            DataJsonOptions);

        var userMessage = $"<analysis_data>\n{data}\n</analysis_data>";

        var payload = await client.CompleteAsync<RecommendationPayload>(
            "recommendations", Prompts.RecommendationsSystem, userMessage, Prompts.RecommendationsSchema, ct);

        return new RecommendationResult(payload.Summary ?? string.Empty, payload.Recommendations ?? []);
    }
}