using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Ai;

namespace CareerAI.Application.Interfaces;

// AI sağlayıcısı soyutlaması. Application, OpenAI'ı bilmez.
public interface ISkillExtractor
{
    Task<CvSkillExtraction> ExtractCvSkillsAsync(string resumeText, CancellationToken ct = default);

    Task<JobRequirementExtraction> ExtractJobRequirementsAsync(
        string jobTitle, string jobDescription, CancellationToken ct = default);
}