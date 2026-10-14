using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Interfaces;

namespace CareerAI.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(FileStorageSettings settings)
    {
        _rootPath = Path.GetFullPath(settings.RootPath).TrimEnd(Path.DirectorySeparatorChar);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task SaveAsync(string storedFileName, Stream content, CancellationToken ct = default)
    {
        var path = ResolvePath(storedFileName);

        // CreateNew: aynı adlı bir dosyanın üzerine asla yazma
        await using var file = new FileStream(
            path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(file, ct);
    }

    public Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken ct = default)
    {
        var path = ResolvePath(storedFileName);
        if (!File.Exists(path))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken ct = default)
    {
        File.Delete(ResolvePath(storedFileName)); // dosya yoksa hata vermez
        return Task.CompletedTask;
    }

    // Savunma katmanı: dosya adını biz üretiyoruz ama yine de yükleme klasörünün dışına çıkmasına izin vermiyoruz
    private string ResolvePath(string storedFileName)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storedFileName));

        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid storage path.");

        return fullPath;
    }
}