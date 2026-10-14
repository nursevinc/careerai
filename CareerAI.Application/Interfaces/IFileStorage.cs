using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Application.Interfaces;

public interface IFileStorage
{
    Task SaveAsync(string storedFileName, Stream content, CancellationToken ct = default);

    // Dosya yoksa null döner
    Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken ct = default);

    Task DeleteAsync(string storedFileName, CancellationToken ct = default);
}