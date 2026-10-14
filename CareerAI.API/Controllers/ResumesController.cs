using CareerAI.API.Extensions;
using CareerAI.Application.Common;
using CareerAI.Application.DTOs.Ai;
using CareerAI.Application.DTOs.Resumes;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CareerAI.API.Controllers;

[ApiController]
[Authorize] // bu controller'daki HER endpoint giriş gerektirir
[Route("api/resumes")]
public class ResumesController(
    IResumeService resumeService,
    ISkillExtractionService skillExtractionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResumeDto>>> GetAll(CancellationToken ct)
    {
        var resumes = await resumeService.GetAllAsync(User.GetUserId(), ct);
        return Ok(resumes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResumeDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var resume = await resumeService.GetByIdAsync(User.GetUserId(), id, ct);
        return Ok(resume);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var file = await resumeService.OpenFileAsync(User.GetUserId(), id, ct);

        //  dosya adı verildiği için "Content-Disposition: attachment" ekler: tarayıcıda açılmaz, indirilir
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(file.Content, "application/pdf", file.FileName);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ResumeLimits.MaxRequestBodyBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ResumeLimits.MaxRequestBodyBytes)]
    [ProducesResponseType<ResumeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResumeDto>> Upload(IFormFile? file, CancellationToken ct)
    {
        if (file is null)
            throw new BusinessRuleException("file_required", "No file was uploaded.");

        await using var stream = file.OpenReadStream();

        var resume = await resumeService.UploadAsync(
            User.GetUserId(),
            new UploadResumeRequest(stream, file.FileName, file.Length),
            ct);

        return StatusCode(StatusCodes.Status201Created, resume);
    }

    // AI çağrısı yapar (para harcar): hız sınırı uygulanır
    [HttpPost("{id:guid}/extract-skills")]
    [EnableRateLimiting("ai")]
    [ProducesResponseType<ResumeSkillsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ResumeSkillsDto>> ExtractSkills(Guid id, CancellationToken ct)
    {
        var result = await skillExtractionService.ExtractResumeSkillsAsync(User.GetUserId(), id, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await resumeService.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}