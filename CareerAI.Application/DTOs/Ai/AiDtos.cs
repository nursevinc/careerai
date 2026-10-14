using System;
using System.Collections.Generic;
using System.Text;
namespace CareerAI.Application.DTOs.Ai;

// Modelin CV'den çıkardığı tek bir beceri.
// Name: standart İngilizce ad (eşleştirmede kullanılır). Evidence: CV'den birebir alıntı (uydurma kontrolünde kullanılır).
public record ExtractedSkill(string Name, string Evidence);

// AI sağlayıcısından gelen HAM sonuçlar: henüz temizlenmemiş ve doğrulanmamış
public record CvSkillExtraction(IReadOnlyList<ExtractedSkill> Skills);

public record JobRequirementExtraction(
    IReadOnlyList<string> RequiredSkills,
    IReadOnlyList<string> PreferredSkills,
    string ExperienceLevel,
    IReadOnlyList<string> OtherRequirements);

// API'nin döndüğü, temizlenmiş ve kaydedilmiş sonuçlar
public record ResumeSkillsDto(Guid ResumeId, IReadOnlyList<string> Skills);

public record JobRequirementsDto(
    Guid JobApplicationId,
    IReadOnlyList<string> RequiredSkills,
    IReadOnlyList<string> PreferredSkills,
    string ExperienceLevel,
    IReadOnlyList<string> OtherRequirements);