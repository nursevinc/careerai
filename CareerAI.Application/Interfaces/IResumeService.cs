using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Resumes;

namespace CareerAI.Application.Interfaces;

public interface IResumeService
{
    Task<ResumeDto> UploadAsync(Guid userId, UploadResumeRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ResumeDto>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<ResumeDetailDto> GetByIdAsync(Guid userId, Guid resumeId, CancellationToken ct = default);
    Task<ResumeFile> OpenFileAsync(Guid userId, Guid resumeId, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid resumeId, CancellationToken ct = default);
}