using System;
using System.Collections.Generic;
using System.Text;
using CareerAI.Domain.Entities;

namespace CareerAI.Application.Interfaces;

public record AccessToken(string Token, DateTime ExpiresAtUtc);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
}