using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Application.DTOs.JobApplications;

public record CreateJobApplicationRequest(string JobTitle, string CompanyName, string JobDescription);

// Liste için: uzun ilan metni yok. MatchScore null = henüz analiz edilmedi.
public record JobApplicationDto(Guid Id, string JobTitle, string CompanyName, int? MatchScore, DateTime CreatedAt);

public record JobApplicationDetailDto(
    Guid Id,
    string JobTitle,
    string CompanyName,
    string JobDescription,
    int? MatchScore,
    DateTime CreatedAt);