using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Account;
using CareerAI.Application.DTOs.Auth;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;
using CareerAI.Domain.Entities;
using CareerAI.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareerAI.Infrastructure.Services;

public class AccountService(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IFileStorage fileStorage,
    IValidator<UpdateProfileRequest> profileValidator,
    IValidator<ChangePasswordRequest> changePasswordValidator,
    IValidator<DeleteAccountRequest> deleteValidator,
    ILogger<AccountService> logger) : IAccountService
{
    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        await profileValidator.ValidateAndThrowAsync(request, ct);

        var user = await FindUserAsync(userId, ct);
        user.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);

        return ToDto(user);
    }

    public async Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        await changePasswordValidator.ValidateAndThrowAsync(request, ct);

        var user = await FindUserAsync(userId, ct);

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            logger.LogWarning("Password change rejected: wrong current password (user {UserId})", userId);
            throw new BusinessRuleException("invalid_password", "The current password is incorrect.");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid(); // diğer cihazlardaki tüm oturumlar anında geçersiz olur
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Password changed (user {UserId})", userId);

        // İşlemi yapan cihaz yeni damgalı token alır ve oturumu açık kalır
        var token = tokenService.CreateAccessToken(user);
        return new AuthResponse(token.Token, token.ExpiresAtUtc, ToDto(user));
    }

    public async Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken ct = default)
    {
        await deleteValidator.ValidateAndThrowAsync(request, ct);

        var user = await FindUserAsync(userId, ct);

        // Açık bir oturum yetmez: şifre ile yeniden doğrulama
        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Account deletion rejected: wrong password (user {UserId})", userId);
            throw new BusinessRuleException("invalid_password", "The password is incorrect.");
        }

        // Dosya adlarını silmeden ÖNCE al: satırlar gidince erişemeyiz
        var storedFiles = await db.Resumes
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => r.StoredFileName)
            .ToListAsync(ct);

        // Tek transaction, bağımlılık sırasıyla. (EnableRetryOnFailure açıkken elle transaction execution strategy içinde olmalı.)
        // AIAnalysis -> Resume ilişkisi Restrict olduğu için analizler CV'lerden önce silinir.
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            await db.AIAnalyses.Where(a => a.JobApplication.UserId == userId).ExecuteDeleteAsync(ct);
            await db.JobApplications.Where(a => a.UserId == userId).ExecuteDeleteAsync(ct); // beceri kayıtları cascade ile gider
            await db.Resumes.Where(r => r.UserId == userId).ExecuteDeleteAsync(ct);         // CV becerileri cascade ile gider
            await db.Users.Where(u => u.Id == userId).ExecuteDeleteAsync(ct);

            await transaction.CommitAsync(ct);
        });

        // Veritabanı silindikten sonra dosyalar. Silinemezse yetim dosya kalır: hata olarak logla (CV içerir, temizlenmeli).
        foreach (var storedFile in storedFiles)
        {
            try
            {
                await fileStorage.DeleteAsync(storedFile, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "A stored resume file of a deleted account could not be removed (user {UserId})", userId);
            }
        }

        logger.LogInformation("Account deleted (user {UserId}, {FileCount} resume file(s))", userId, storedFiles.Count);
    }

    private async Task<User> FindUserAsync(Guid userId, CancellationToken ct) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
        ?? throw new NotFoundException("User not found.");

    private static UserDto ToDto(User user) => new(user.Id, user.Name, user.Email, user.CreatedAt);
}