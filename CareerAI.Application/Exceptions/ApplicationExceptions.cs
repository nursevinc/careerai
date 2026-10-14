using System;
using System.Collections.Generic;
using System.Text;
namespace CareerAI.Application.Exceptions;

// 409: kaynak zaten var / çakışma
public class ConflictException(string message) : Exception(message);

// 401: kimlik doğrulama başarısız
public class AuthenticationFailedException(string message) : Exception(message);

// 404: kayıt bulunamadı (ya da kullanıcının erişimi yok)
public class NotFoundException(string message) : Exception(message);

// 400: iş kuralı ihlali. Code, frontend'in çeviri için kullandığı makine-okunur koddur (örn. "file_not_pdf").
public class BusinessRuleException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

// AI sağlayıcısı (OpenAI) kaynaklı hata.
// Code: ai_busy | ai_unavailable | ai_refused | ai_invalid_response
public class AiServiceException(string code, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public string Code { get; } = code;
}