using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Domain.Common;

namespace CareerAI.Domain.Entities;

public class AIAnalysis : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public Guid ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;

    // AI önerileri vb. JSON string olarak. Sorgulamayacağımız, değişebilir veri.
    public string AnalysisResult { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}