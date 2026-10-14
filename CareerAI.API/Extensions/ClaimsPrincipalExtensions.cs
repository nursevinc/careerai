using System.Security.Claims;
using CareerAI.Application.Exceptions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CareerAI.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    // Token'daki "sub" claim'inden giriş yapmış kullanıcının ID'sini okur.
    // Kullanıcı ID'sini url'den almıyoruz. token'dan alıyoruz (user isolation).
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(value, out var id)
            ? id
            : throw new AuthenticationFailedException("Invalid token.");
    }
}