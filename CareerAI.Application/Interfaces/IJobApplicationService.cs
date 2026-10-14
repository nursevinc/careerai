using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.JobApplications;

namespace CareerAI.Application.Interfaces;

public interface IJobApplicationService
{
    Task<JobApplicationDto> CreateAsync(Guid userId, CreateJobApplicationRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<JobApplicationDto>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<JobApplicationDetailDto> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);
}
