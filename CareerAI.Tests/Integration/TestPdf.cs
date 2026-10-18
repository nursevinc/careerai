using System;
using System.Collections.Generic;
using System.Text;
using System.Text;

namespace CareerAI.Tests.Integration;

public static class TestPdf
{
    // Tek sayfalık, geçerli ve metni seçilebilir minimal bir PDF üretir (ASCII metin).
    public static byte[] Create(string text)
    {
        var sb = new StringBuilder();
        var offsets = new List<int>();

        void AddObject(string body)
        {
            offsets.Add(sb.Length);
            sb.Append(offsets.Count).Append(" 0 obj\n").Append(body).Append("\nendobj\n");
        }

        var content = $"BT /F1 12 Tf 72 720 Td ({Escape(text)}) Tj ET";

        sb.Append("%PDF-1.4\n");
        AddObject("<< /Type /Catalog /Pages 2 0 R >>");
        AddObject("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        AddObject("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>");
        AddObject($"<< /Length {content.Length} >>\nstream\n{content}\nendstream");
        AddObject("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        var xrefOffset = sb.Length;
        sb.Append("xref\n0 ").Append(offsets.Count + 1).Append('\n');
        sb.Append("0000000000 65535 f \n");
        foreach (var offset in offsets)
            sb.Append(offset.ToString("D10")).Append(" 00000 n \n");

        sb.Append("trailer\n<< /Size ").Append(offsets.Count + 1).Append(" /Root 1 0 R >>\n");
        sb.Append("startxref\n").Append(xrefOffset).Append("\n%%EOF");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private static string Escape(string text) =>
        text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}