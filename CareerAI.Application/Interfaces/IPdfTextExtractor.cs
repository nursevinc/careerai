using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Application.Interfaces;

public interface IPdfTextExtractor
{
    /// <summary>PDF'ten düz metin çıkarır. Okunamayan PDF için BusinessRuleException fırlatır.</summary>
    string ExtractText(Stream pdfStream);
}