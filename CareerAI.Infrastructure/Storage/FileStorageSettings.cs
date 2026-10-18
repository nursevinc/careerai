using System;
using System.Collections.Generic;
using System.Text;
namespace CareerAI.Infrastructure.Storage;


public class FileStorageSettings
{
    public const string SectionName = "Storage";

    // "Local": sunucu diski (geliştirme). "Database": dosyalar veritabanında
    // (kalıcı diski olmayan ortamlar, örn. Render ücretsiz plan).
    public string Provider { get; set; } = "Local";

    // Sadece Local için. Göreli ise Program.cs bunu uygulamanın içerik klasörüne göre mutlak yola çevirir
    public string RootPath { get; set; } = "uploads";
}