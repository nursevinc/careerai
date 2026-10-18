using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Infrastructure.Auth;

public class RegistrationSettings
{
    public const string SectionName = "Registration";

    // 0 = sınırsız. Herkese açık demoda disk ve AI maliyetine tavan koyar.
    public int MaxUsers { get; set; }
}