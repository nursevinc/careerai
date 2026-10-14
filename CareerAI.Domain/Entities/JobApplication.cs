using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Domain.Common;

namespace CareerAI.Domain.Entities;

public class JobApplication : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;

    // Analiz yapılana kadar null. 0-100 arası.
    public int? MatchScore { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<JobApplicationSkill> Skills { get; set; } = new List<JobApplicationSkill>();
    public ICollection<AIAnalysis> Analyses { get; set; } = new List<AIAnalysis>();
}