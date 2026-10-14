using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Interfaces;
using CareerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerAI.Infrastructure.Auth;

public class UserSessionValidator(AppDbContext db) : IUserSessionValidator
{
    public async Task<bool> IsSessionValidAsync(Guid userId, Guid securityStamp, CancellationToken ct = default)
    {
        // Birincil anahtar araması, tek sütun: milisaniyeler. Kullanıcı silinmişse null döner.
        var currentStamp = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => (Guid?)u.SecurityStamp)
            .FirstOrDefaultAsync(ct);

        return currentStamp is not null && currentStamp == securityStamp;
    }
}