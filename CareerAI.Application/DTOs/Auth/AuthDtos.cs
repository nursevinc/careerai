using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Application.DTOs.Auth;

public record RegisterRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record UserDto(Guid Id, string Name, string Email, DateTime CreatedAt);

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserDto User);