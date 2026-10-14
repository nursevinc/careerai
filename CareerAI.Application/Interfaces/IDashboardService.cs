using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Dashboard;

namespace CareerAI.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(Guid userId, CancellationToken ct = default);
}