using System;
using System.Collections.Generic;
using System.Text;
using CareerAI.Domain.Common;


namespace CareerAI.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Şifre değişince yenilenir. Token'daki değerle eşleşmeyen oturumlar geçersiz sayılır.
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}