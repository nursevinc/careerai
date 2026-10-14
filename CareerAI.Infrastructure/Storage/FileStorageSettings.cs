using System;
using System.Collections.Generic;
using System.Text;
namespace CareerAI.Infrastructure.Storage;

public class FileStorageSettings
{
    public const string SectionName = "Storage";

    // Göreli ise Program.cs bunu uygulamanın içerik klasörüne göre mutlak yola çevirir
    public string RootPath { get; set; } = "uploads";
}