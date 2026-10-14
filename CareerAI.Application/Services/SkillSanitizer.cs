using System;
using System.Collections.Generic;
using System.Text;
using CareerAI.Application.DTOs.Ai;
using System.Text.RegularExpressions;

namespace CareerAI.Application.Services;

// AI çıktısını doğrulayıp temizleyen saf mantık. AI'a körü körüne güvenmiyoruz.
public static class SkillSanitizer
{
    public const int MaxSkillLength = 50;
    public const int MaxSkills = 80;
    public const int MaxOtherRequirements = 10;
    public const int MaxOtherRequirementLength = 120;

    private static readonly string[] ExperienceLevels = ["Intern", "Junior", "Mid", "Senior", "Lead", "Unspecified"];
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    // Boşlukları düzeltir, boş/çok uzun olanları ve tekrarları (büyük-küçük harf farkı olmadan) atar, sayıyı sınırlar
    public static List<string> CleanList(IEnumerable<string?>? items, int maxCount, int maxLength)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in items ?? [])
        {
            var item = NormalizeText(raw);
            if (item.Length == 0 || item.Length > maxLength) continue;
            if (!seen.Add(item)) continue;

            result.Add(item);
            if (result.Count >= maxCount) break;
        }

        return result;
    }

    public static List<string> CleanSkills(IEnumerable<string?>? skills) =>
        CleanList(skills, MaxSkills, MaxSkillLength);

    // "exclude" listesindeki öğeleri çıkarır (örn. hem required hem preferred'da geçen beceri)
    public static List<string> RemoveOverlap(IEnumerable<string> items, IEnumerable<string> exclude)
    {
        var excluded = new HashSet<string>(exclude, StringComparer.OrdinalIgnoreCase);
        return items.Where(item => !excluded.Contains(item)).ToList();
    }

    public static string NormalizeExperienceLevel(string? level) =>
        ExperienceLevels.FirstOrDefault(l => string.Equals(l, level?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? "Unspecified";

    // Uydurma kontrolü: kaynak metinde geçmeyen beceriyi atar.
    // Karşılaştırmada büyük-küçük harf, boşluk ve noktalama farkları göz ardı edilir ("ASP.NET Core" == "asp.net core").
    // Uydurma kontrolü: kanıtı (evidence) kaynak metinde geçmeyen beceriyi atar.
    // Karşılaştırmada büyük-küçük harf, boşluk ve noktalama farkları göz ardı edilir ("ASP.NET Core" == "asp.net core").
    public static List<ExtractedSkill> KeepGrounded(
        IEnumerable<ExtractedSkill> skills, string sourceText, out int droppedCount)
    {
        var haystack = Compact(sourceText);
        var kept = new List<ExtractedSkill>();
        droppedCount = 0;

        foreach (var skill in skills)
        {
            var needle = Compact(skill.Evidence);
            if (needle.Length > 0 && haystack.Contains(needle, StringComparison.Ordinal))
                kept.Add(skill);
            else
                droppedCount++;
        }

        return kept;
    }

    private static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var withoutControls = new string(value.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray());
        return Whitespace.Replace(withoutControls, " ").Trim();
    }

    // Sadece harf, rakam ve # + . karakterlerini küçük harfle bırakır
    private static string Compact(string value) =>
        new(value.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || c is '#' or '+' or '.').ToArray());
}