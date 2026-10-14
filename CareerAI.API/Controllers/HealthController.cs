using Microsoft.AspNetCore.Mvc;

namespace CareerAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "Healthy",
        service = "CareerAI.API",
        timestampUtc = DateTime.UtcNow
    });
}