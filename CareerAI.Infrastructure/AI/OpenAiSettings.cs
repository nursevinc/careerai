using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Infrastructure.AI;

public class OpenAiSettings
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty; // gizli: User Secrets / env var (OpenAI__ApiKey)
    public string Model { get; set; } = "gpt-4.1-mini";
    public int MaxOutputTokens { get; set; } = 2000;
    public int TimeoutSeconds { get; set; } = 60;

    // null ise gönderilmez. gpt-5 serisi gibi akıl yürütme (reasoning) modelleri temperature kabul etmez:
    // onlara geçersen appsettings.json'daki Temperature satırını sil.
    public float? Temperature { get; set; }
}