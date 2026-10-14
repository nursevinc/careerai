using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Domain.Common;

namespace CareerAI.Domain.Entities;

public class Resume : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Kullanıcının yüklediği orijinal ad (sadece ekranda göstermek için)
    public string FileName { get; set; } = string.Empty;

    // Diskte kullanacağımız güvenli, sistem üretimi ad (örn. "3f2a...c1.pdf")
    public string StoredFileName { get; set; } = string.Empty;

    public string ExtractedText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ResumeSkill> Skills { get; set; } = new List<ResumeSkill>();
}