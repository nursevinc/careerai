using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Domain.Common;

namespace CareerAI.Domain.Entities;

// DatabaseFileStorage'ın kaydı: dosya içeriği veritabanında tutulur
public class StoredFile : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Resume.StoredFileName ile eşleşir
    public byte[] Content { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}