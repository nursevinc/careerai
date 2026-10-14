using System;
using System.Collections.Generic;
using System.Text;
using CareerAI.Application.DTOs.Ai;

namespace CareerAI.Application.Interfaces;

public interface ISkillExtractionService
{
    // CV'den becerileri çıkarır ve veritabanına kaydeder (önceki sonucun yerine geçer)
    Task<ResumeSkillsDto> ExtractResumeSkillsAsync(Guid userId, Guid resumeId, CancellationToken ct = default);

    // İş ilanından gereksinimleri çıkarır becerileri veritabanına kaydeder
    Task<JobRequirementsDto> ExtractJobRequirementsAsync(Guid userId, Guid jobApplicationId, CancellationToken ct = default);
}