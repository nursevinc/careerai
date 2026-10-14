using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Analysis;

namespace CareerAI.Application.Interfaces;

public interface IAnalysisService
{
    Task<AnalysisResultDto> AnalyzeAsync(
        Guid userId, Guid jobApplicationId, AnalyzeRequest request, CancellationToken ct = default);

    // İlanın en son analizi (yoksa 404)
    Task<AnalysisResultDto> GetLatestAsync(Guid userId, Guid jobApplicationId, CancellationToken ct = default);
}