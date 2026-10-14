using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Application.DTOs.Resumes;

// Controller'ın IFormFile'ını servise taşıyan, ASP.NET'ten bağımsız istek nesnesi
public record UploadResumeRequest(Stream Content, string FileName, long Length);

public record ResumeDto(Guid Id, string FileName, DateTime CreatedAt, int ExtractedTextLength);

public record ResumeDetailDto(
    Guid Id,
    string FileName,
    DateTime CreatedAt,
    int ExtractedTextLength,
    string ExtractedText);

public record ResumeFile(Stream Content, string FileName);