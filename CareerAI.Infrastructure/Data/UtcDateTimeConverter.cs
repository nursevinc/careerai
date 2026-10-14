using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CareerAI.Infrastructure.Data;

// Veritabanından okunan DateTime'ı UTC olarak işaretler. JSON'da "...Z" ile gider, tarayıcı doğru çevirir.
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}