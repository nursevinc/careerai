using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Analysis;

namespace CareerAI.Application.Interfaces;

public interface IRecommendationGenerator
{
    Task<RecommendationResult> GenerateAsync(RecommendationRequest request, CancellationToken ct = default);
}