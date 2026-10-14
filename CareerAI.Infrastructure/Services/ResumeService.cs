using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Common;
using CareerAI.Application.DTOs.Resumes;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;
using CareerAI.Domain.Entities;
using CareerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareerAI.Infrastructure.Services;

public class ResumeService(
    AppDbContext db,
    IFileStorage storage,
    IPdfTextExtractor pdfTextExtractor,
    ILogger<ResumeService> logger) : IResumeService
{
    private static readonly char[] ForbiddenNameChars = ['<', '>', ':', '"', '|', '?', '*'];

    public async Task<ResumeDto> UploadAsync(Guid userId, UploadResumeRequest request, CancellationToken ct = default)
    {
        // 1) Boyut
        if (request.Length == 0)
            throw new BusinessRuleException("file_required", "No file was uploaded or the file is empty.");

        if (request.Length > ResumeLimits.MaxFileSizeBytes)
            throw new BusinessRuleException("file_too_large", "The file exceeds the maximum allowed size.");

        // 2) Kota
        var resumeCount = await db.Resumes.CountAsync(r => r.UserId == userId, ct);
        if (resumeCount >= ResumeLimits.MaxResumesPerUser)
            throw new BusinessRuleException("resume_limit_reached", "The maximum number of resumes has been reached.");

        // 3) Uzantı (güvenilmez ama ucuz bir ilk eleme)
        var displayName = SanitizeFileName(request.FileName);
        if (!displayName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("file_not_pdf", "Only PDF files are accepted.");

        // 4) Magic bytes: asıl içerik kontrolü. Dosyayı belleğe alıyoruz (en fazla 5 MB) ki birden çok kez okuyabilelim.
        await using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, ct);

        buffer.Position = 0;
        if (!HasPdfSignature(buffer))
            throw new BusinessRuleException("file_not_pdf", "Only PDF files are accepted.");

        // 5) Gerçek ayrıştırma: bozuk, şifreli ya da çok sayfalı PDF burada elenir
        buffer.Position = 0;
        var text = pdfTextExtractor.ExtractText(buffer);

        if (string.IsNullOrWhiteSpace(text))
            throw new BusinessRuleException(
                "pdf_no_text",
                "No text could be extracted. The PDF may be a scanned image.");

        // 6) Buraya kadar geldiyse dosya geçerli: kaydet. Diskteki ad sistemin ürettiği GUID, kullanıcının adı değil.
        var storedFileName = $"{Guid.NewGuid():N}.pdf";
        buffer.Position = 0;
        await storage.SaveAsync(storedFileName, buffer, ct);

        var resume = new Resume
        {
            UserId = userId,
            FileName = displayName,
            StoredFileName = storedFileName,
            ExtractedText = text
        };

        db.Resumes.Add(resume);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // Veritabanı kaydı başarısızsa yetim dosya bırakma
            await storage.DeleteAsync(storedFileName, CancellationToken.None);
            throw;
        }

        logger.LogInformation("Resume uploaded: {ResumeId} (user {UserId}, {Length} chars)",
            resume.Id, userId, text.Length);

        return new ResumeDto(resume.Id, resume.FileName, resume.CreatedAt, text.Length);
    }

    public async Task<IReadOnlyList<ResumeDto>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        // Her sorguda UserId filtresi: kullanıcı izolasyonu. Length SQL'de hesaplanır, metin belleğe çekilmez.
        return await db.Resumes
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ResumeDto(r.Id, r.FileName, r.CreatedAt, r.ExtractedText.Length))
            .ToListAsync(ct);
    }

    public async Task<ResumeDetailDto> GetByIdAsync(Guid userId, Guid resumeId, CancellationToken ct = default)
    {
        // Başkasının CV'si için de "bulunamadı" dönüyoruz (varlığını sızdırmamak için 403 değil 404)
        var resume = await db.Resumes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId, ct)
            ?? throw new NotFoundException("Resume not found.");

        return new ResumeDetailDto(
            resume.Id, resume.FileName, resume.CreatedAt, resume.ExtractedText.Length, resume.ExtractedText);
    }

    public async Task<ResumeFile> OpenFileAsync(Guid userId, Guid resumeId, CancellationToken ct = default)
    {
        var resume = await db.Resumes
            .AsNoTracking()
            .Where(r => r.Id == resumeId && r.UserId == userId)
            .Select(r => new { r.StoredFileName, r.FileName })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Resume not found.");

        var stream = await storage.OpenReadAsync(resume.StoredFileName, ct);
        if (stream is null)
        {
            logger.LogError("Stored file is missing for resume {ResumeId}", resumeId);
            throw new NotFoundException("The resume file is no longer available.");
        }

        return new ResumeFile(stream, resume.FileName);
    }

    public async Task DeleteAsync(Guid userId, Guid resumeId, CancellationToken ct = default)
    {
        var resume = await db.Resumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId, ct)
            ?? throw new NotFoundException("Resume not found.");

        // AIAnalysis -> Resume ilişkisi Restrict: analizi olan CV'yi silmeye çalışırsak veritabanı hata verir
        if (await db.AIAnalyses.AnyAsync(a => a.ResumeId == resumeId, ct))
            throw new ConflictException("This resume is used by existing analyses and cannot be deleted.");

        db.Resumes.Remove(resume);
        await db.SaveChangesAsync(ct);

        // Önce veritabanı, sonra dosya: dosya silinemezse zararsız bir yetim dosya kalır,
        // tersi olsaydı veritabanında olmayan bir dosyaya işaret eden kayıt kalırdı.
        await storage.DeleteAsync(resume.StoredFileName, ct);

        logger.LogInformation("Resume deleted: {ResumeId} (user {UserId})", resumeId, userId);
    }

    private static bool HasPdfSignature(Stream stream)
    {
        Span<byte> header = stackalloc byte[5];
        var read = stream.Read(header);
        return read == 5 && header.SequenceEqual("%PDF-"u8);
    }

    // Kullanıcının gönderdiği ad sadece ekranda gösterilir, diske yazılmaz. Yine de temizliyoruz.
    private static string SanitizeFileName(string? fileName)
    {
        // Bazı tarayıcılar tam yol gönderir ("C:\Users\x\cv.pdf"): sadece dosya adı kısmını al
        var name = Path.GetFileName((fileName ?? string.Empty).Replace('\\', '/'));

        name = new string(name.Where(c => !char.IsControl(c) && !ForbiddenNameChars.Contains(c)).ToArray()).Trim();

        if (name.Length == 0)
            name = "resume.pdf";

        if (name.Length > 255)
            name = name[^255..]; // sondan al: uzantı korunur

        return name;
    }
}