using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.JobApplications;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;
using CareerAI.Domain.Entities;
using CareerAI.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareerAI.Infrastructure.Services;

public class JobApplicationService(
    AppDbContext db,
    IValidator<CreateJobApplicationRequest> validator,
    ILogger<JobApplicationService> logger) : IJobApplicationService
{
    public async Task<JobApplicationDto> CreateAsync(
        Guid userId, CreateJobApplicationRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var application = new JobApplication
        {
            UserId = userId, // token'dan gelen kullanıcı, istekten değil
            JobTitle = request.JobTitle.Trim(),
            CompanyName = request.CompanyName.Trim(),
            JobDescription = request.JobDescription.Trim()
        };

        db.JobApplications.Add(application);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Job application created: {ApplicationId} (user {UserId})", application.Id, userId);

        return new JobApplicationDto(
            application.Id, application.JobTitle, application.CompanyName, application.MatchScore, application.CreatedAt);
    }

    public async Task<IReadOnlyList<JobApplicationDto>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        // Her sorguda UserId filtresi: kullanıcı izolasyonu. İlan metni listeye çekilmiyor.
        return await db.JobApplications
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new JobApplicationDto(a.Id, a.JobTitle, a.CompanyName, a.MatchScore, a.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<JobApplicationDetailDto> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        // Başkasının ilanı için de 404 (varlığını sızdırmamak için 403 değil)
        var application = await db.JobApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct)
            ?? throw new NotFoundException("Job application not found.");

        return new JobApplicationDetailDto(
            application.Id,
            application.JobTitle,
            application.CompanyName,
            application.JobDescription,
            application.MatchScore,
            application.CreatedAt);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var application = await db.JobApplications
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct)
            ?? throw new NotFoundException("Job application not found.");

        // İlişkili beceri kayıtları ve analizler veritabanında cascade ile silinir
        db.JobApplications.Remove(application);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Job application deleted: {ApplicationId} (user {UserId})", id, userId);
    }
}