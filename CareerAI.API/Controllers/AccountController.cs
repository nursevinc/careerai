using CareerAI.API.Extensions;
using CareerAI.Application.DTOs.Account;
using CareerAI.Application.DTOs.Auth;
using CareerAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CareerAI.API.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public class AccountController(IAccountService accountService) : ControllerBase
{
    [HttpPut("profile")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileRequest request, CancellationToken ct)
    {
        return Ok(await accountService.UpdateProfileAsync(User.GetUserId(), request, ct));
    }

    // Şifre doğrulama BCrypt ile yapılır: kaba kuvvet denemesine karşı hız sınırı
    [HttpPost("change-password")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        return Ok(await accountService.ChangePasswordAsync(User.GetUserId(), request, ct));
    }

    [HttpDelete]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(DeleteAccountRequest request, CancellationToken ct)
    {
        await accountService.DeleteAccountAsync(User.GetUserId(), request, ct);
        return NoContent();
    }
}