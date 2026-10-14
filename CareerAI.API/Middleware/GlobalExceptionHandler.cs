using CareerAI.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CareerAI.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // İstemci bağlantıyı kapattıysa (sayfa kapandı, istek iptal edildi) yanıtı okuyacak kimse yok. hata değil, bilgi
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request cancelled by the client on {Path}", httpContext.Request.Path);
            return true;
        }
        int status;
        string title;
        string detail;
        string? code = null;
        Dictionary<string, string[]>? errors = null;

        switch (exception)
        {
            case ValidationException validationException:
                status = StatusCodes.Status400BadRequest;
                title = "Validation failed";
                detail = "One or more validation errors occurred.";
                errors = validationException.Errors
                    .GroupBy(e => ToCamelCase(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;
            case BusinessRuleException businessRule:
                status = StatusCodes.Status400BadRequest;
                title = "Request rejected";
                detail = businessRule.Message;
                code = businessRule.Code;
                break;
            case AiServiceException aiException:
                // 502: AI'dan gelen yanıt kullanılamaz, 422: model reddetti, 503: AI servisi meşgul/ulaşılamıyor
                status = aiException.Code switch
                {
                    "ai_invalid_response" => StatusCodes.Status502BadGateway,
                    "ai_refused" => StatusCodes.Status422UnprocessableEntity,
                    _ => StatusCodes.Status503ServiceUnavailable
                };
                title = "AI service error";
                detail = aiException.Message;
                code = aiException.Code;
                break;
            case ConflictException:
                status = StatusCodes.Status409Conflict;
                title = "Conflict";
                detail = exception.Message;
                break;
            case AuthenticationFailedException:
                status = StatusCodes.Status401Unauthorized;
                title = "Authentication failed";
                detail = exception.Message;
                break;
            case NotFoundException:
                status = StatusCodes.Status404NotFound;
                title = "Not found";
                detail = exception.Message;
                break;
            default:
                status = StatusCodes.Status500InternalServerError;
                title = "Server error";
                detail = "An unexpected error occurred."; // iç detayı istemciye sızdırma
                break;
        }

        if (status >= 500)
            logger.LogError(exception, "Request failed with {Status} on {Path}", status, httpContext.Request.Path);
        else
            logger.LogWarning("Handled {ExceptionType} on {Path}: {Message}",
                exception.GetType().Name, httpContext.Request.Path, exception.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        if (code is not null)
            problem.Extensions["code"] = code;
        if (errors is not null)
            problem.Extensions["errors"] = errors;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);

        return true;
    }

    // "Email" -> "email": frontend form alanlarıyla aynı isim olsun
    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}