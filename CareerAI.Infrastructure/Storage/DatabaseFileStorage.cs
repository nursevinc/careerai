using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Interfaces;
using CareerAI.Domain.Entities;
using CareerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerAI.Infrastructure.Storage;

// Dosyaları veritabanında (varbinary) saklar. Küçük dosyalar (en fazla 5 MB) ve düşük hacim için uygundur.
// Ölçeklenince blob storage'a geçilir: aynı IFileStorage arayüzü, iş mantığı değişmez.
public class DatabaseFileStorage(AppDbContext db) : IFileStorage
{
    public async Task SaveAsync(string storedFileName, Stream content, CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);

        db.StoredFiles.Add(new StoredFile { Name = storedFileName, Content = buffer.ToArray() });
        await db.SaveChangesAsync(ct);
    }

    public async Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken ct = default)
    {
        var bytes = await db.StoredFiles
            .AsNoTracking()
            .Where(f => f.Name == storedFileName)
            .Select(f => f.Content)
            .FirstOrDefaultAsync(ct);

        return bytes is null ? null : new MemoryStream(bytes, writable: false);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken ct = default) =>
        db.StoredFiles.Where(f => f.Name == storedFileName).ExecuteDeleteAsync(ct);
}