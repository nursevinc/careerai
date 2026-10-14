using CareerAI.API.Extensions;
using CareerAI.Application.DTOs.Dashboard;
using CareerAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerAI.API.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    // Kişiye özel veri: ara sunucular (proxy) önbelleğe almasınb
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType<DashboardDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
    {
        return Ok(await dashboardService.GetAsync(User.GetUserId(), ct));
    }
}