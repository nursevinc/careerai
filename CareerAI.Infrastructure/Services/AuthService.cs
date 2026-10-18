using CareerAI.Application.DTOs.Auth;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;
using CareerAI.Domain.Entities;
using CareerAI.Infrastructure.Auth;
using CareerAI.Infrastructure.Data;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareerAI.Infrastructure.Services;

public class AuthService(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    RegistrationSettings registrationSettings,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        await registerValidator.ValidateAndThrowAsync(request, ct);

        // Herkese açık demoda toplam kullanıcı sayısına tavan
        if (registrationSettings.MaxUsers > 0 && await db.Users.CountAsync(ct) >= registrationSettings.MaxUsers)
            throw new BusinessRuleException("registration_closed", "New registrations are currently closed.");

        var email = NormalizeEmail(request.Email);

        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // İki istek aynı anda geldiyse unique index bizi korur (yarış durumu)
            throw new ConflictException("An account with this email already exists.");
        }

        logger.LogInformation("User registered: {UserId}", user.Id);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        await loginValidator.ValidateAndThrowAsync(request, ct);

        var email = NormalizeEmail(request.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        // Kullanıcı yoksa da bir hash doğrulaması yapıyoruz: yanıt süresi e-postanın kayıtlı olup olmadığını sızdırmasın
        var passwordValid = user is null
            ? passwordHasher.SimulateVerify(request.Password)
            : passwordHasher.Verify(request.Password, user.PasswordHash);

        // Kullanıcıya hep aynı mesaj. Sebep sadece logda, e-posta adresi (kişisel veri) loglanmıyor.
        if (user is null || !passwordValid)
        {
            logger.LogWarning("Failed login attempt ({Reason})", user is null ? "unknown_email" : "wrong_password");
            throw new AuthenticationFailedException("Invalid email or password.");
        }

        return CreateAuthResponse(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException("User not found.");

        return ToDto(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = tokenService.CreateAccessToken(user);
        return new AuthResponse(token.Token, token.ExpiresAtUtc, ToDto(user));
    }

    private static UserDto ToDto(User u) => new(u.Id, u.Name, u.Email, u.CreatedAt);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}