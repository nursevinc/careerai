using CareerAI.API.Extensions;
using CareerAI.Application.DTOs.Ai;
using CareerAI.Application.DTOs.Analysis;
using CareerAI.Application.DTOs.JobApplications;
using CareerAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CareerAI.API.Controllers;

[ApiController]
[Authorize]
[Route("api/job-applications")]
public class JobApplicationsController(
    IJobApplicationService service,
    ISkillExtractionService skillExtractionService,
    IAnalysisService analysisService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<JobApplicationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobApplicationDto>> Create(CreateJobApplicationRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(User.GetUserId(), request, ct);

        // 201 + Location başlığı (yeni kaynağın adresi)
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JobApplicationDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await service.GetAllAsync(User.GetUserId(), ct));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<JobApplicationDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await service.GetByIdAsync(User.GetUserId(), id, ct));
    }

    // ai çağrısı yapar  hız sınırı uygulanır
    [HttpPost("{id:guid}/extract-requirements")]
    [EnableRateLimiting("ai")]
    [ProducesResponseType<JobRequirementsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<JobRequirementsDto>> ExtractRequirements(Guid id, CancellationToken ct)
    {
        var result = await skillExtractionService.ExtractJobRequirementsAsync(User.GetUserId(), id, ct);
        return Ok(result);
    }

    // Ana endpoint: CV ve ilan , beceri çıkarma , eşleştirme , skor, öneriler, kaydet
    [HttpPost("{id:guid}/analyze")]
    [EnableRateLimiting("ai")]
    [ProducesResponseType<AnalysisResultDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AnalysisResultDto>> Analyze(Guid id, AnalyzeRequest request, CancellationToken ct)
    {
        var result = await analysisService.AnalyzeAsync(User.GetUserId(), id, request, ct);
        return CreatedAtAction(nameof(GetAnalysis), new { id }, result);
    }

    // İlanın  son analizi
    [HttpGet("{id:guid}/analysis")]
    [ProducesResponseType<AnalysisResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalysisResultDto>> GetAnalysis(Guid id, CancellationToken ct)
    {
        return Ok(await analysisService.GetLatestAsync(User.GetUserId(), id, ct));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}