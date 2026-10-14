using System;
using System.Collections.Generic;
using System.Text;
namespace CareerAI.Application.Common;

public static class ResumeLimits
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;                 // 5 MB
    public const long MaxRequestBodyBytes = MaxFileSizeBytes + 512 * 1024; // multipart başlıkları için pay
    public const int MaxPages = 10;
    public const int MaxExtractedTextLength = 30_000;                      // ileride AI prompt'una gireceği için sınırlı
    public const int MaxResumesPerUser = 10;
}