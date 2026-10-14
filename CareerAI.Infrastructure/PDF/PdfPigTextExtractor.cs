using System;
using System.Collections.Generic;
using System.Text;

using System.Text;
using System.Text.RegularExpressions;
using CareerAI.Application.Common;
using CareerAI.Application.Exceptions;
using CareerAI.Application.Interfaces;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace CareerAI.Infrastructure.PDF;

public class PdfPigTextExtractor(ILogger<PdfPigTextExtractor> logger) : IPdfTextExtractor
{
    // NUL gibi kontrol karakterleri (satır sonu ve sekme hariç)
    private static readonly Regex ControlChars = new(@"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", RegexOptions.Compiled);
    private static readonly Regex ExcessBlankLines = new(@"\n{3,}", RegexOptions.Compiled);

    public string ExtractText(Stream pdfStream)
    {
        try
        {
            using var document = PdfDocument.Open(pdfStream);

            if (document.NumberOfPages > ResumeLimits.MaxPages)
                throw new BusinessRuleException(
                    "pdf_too_many_pages",
                    $"The PDF has too many pages (maximum {ResumeLimits.MaxPages}).");

            var builder = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                // ContentOrderTextExtractor: metni sayfadaki okuma sırasına göre birleştirir (sütunlu CV'lerde de işe yarar)
                builder.AppendLine(ContentOrderTextExtractor.GetText(page));

                if (builder.Length >= ResumeLimits.MaxExtractedTextLength)
                    break;
            }

            return Normalize(builder.ToString());
        }
        catch (BusinessRuleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // CV içeriğini loglamıyoruz, sadece hatanın kendisini
            logger.LogWarning(ex, "PDF text extraction failed.");
            throw new BusinessRuleException(
                "pdf_unreadable",
                "The PDF could not be read. It may be corrupted or password-protected.");
        }
    }

    private static string Normalize(string text)
    {
        text = text.Replace("\r\n", "\n").Replace('\r', '\n');
        text = ControlChars.Replace(text, string.Empty);
        text = ExcessBlankLines.Replace(text, "\n\n").Trim();

        return text.Length > ResumeLimits.MaxExtractedTextLength
            ? text[..ResumeLimits.MaxExtractedTextLength]
            : text;
    }
}