using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Analysis;
using CareerAI.Application.Interfaces;

namespace CareerAI.Application.Services;

// Uyum skorunu AI hesaplamaz: deterministik, test edilebilir C# mantığı.
public class MatchingService : IMatchingService
{
    // Zorunlu beceri, tercih edilen beceriden 3 kat değerli
    public const int RequiredWeight = 3;
    public const int PreferredWeight = 1;

    public MatchResult CalculateMatch(
        IEnumerable<string> candidateSkills,
        IEnumerable<string> requiredSkills,
        IEnumerable<string> preferredSkills)
    {
        var candidateKeys = SkillNormalizer.BuildCandidateKeys(candidateSkills);

        var required = DistinctBySkill(requiredSkills);

        // Hem zorunlu hem tercih edilen listesinde geçen beceri sadece zorunlu sayılır
        var requiredKeys = required.Select(SkillNormalizer.Normalize).ToHashSet();
        var preferred = DistinctBySkill(preferredSkills)
            .Where(skill => !requiredKeys.Contains(SkillNormalizer.Normalize(skill)))
            .ToList();

        bool Has(string skill) => candidateKeys.Contains(SkillNormalizer.Normalize(skill));

        var matchedRequired = required.Where(Has).ToList();
        var missingRequired = required.Where(skill => !Has(skill)).ToList();
        var matchedPreferred = preferred.Where(Has).ToList();
        var missingPreferred = preferred.Where(skill => !Has(skill)).ToList();

        var totalWeight = required.Count * RequiredWeight + preferred.Count * PreferredWeight;
        var matchedWeight = matchedRequired.Count * RequiredWeight + matchedPreferred.Count * PreferredWeight;

        // İlanda hiç beceri yoksa karşılaştıracak bir şey yok: 0
        var score = totalWeight == 0
            ? 0
            : (int)Math.Round(100.0 * matchedWeight / totalWeight, MidpointRounding.AwayFromZero);

        return new MatchResult(score, matchedRequired, missingRequired, matchedPreferred, missingPreferred);
    }

    // Aynı beceriyi farklı yazımlarıbir kez say
    private static List<string> DistinctBySkill(IEnumerable<string> skills)
    {
        var seen = new HashSet<string>();
        var result = new List<string>();

        foreach (var skill in skills)
        {
            var key = SkillNormalizer.Normalize(skill);
            if (key.Length == 0 || !seen.Add(key)) continue;

            result.Add(skill.Trim());
        }

        return result;
    }
}
