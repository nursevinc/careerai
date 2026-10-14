using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Application.Common;

public static class JobApplicationLimits
{
    public const int MaxNameLength = 200;               // pozisyon ve şirket adı
    public const int MinDescriptionLength = 30;
    public const int MaxDescriptionLength = 10_000;     // AI prompt'una gireceği için sınırlı (token maliyeti)
}