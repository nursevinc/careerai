using System.Net;

namespace CareerAI.API.Extensions;

public static class ClientIpExtensions
{
    // Ters vekil (örn. Render) arkasında gerçek istemci IP'sini, vekilin yazdığı başlıktan okur.
    // YALNIZCA uygulamaya sadece o vekil üzerinden erişilebiliyorsa açın: aksi halde istemci başlığı taklit edebilir.
    // Başlık adı boşsa hiçbir şey yapmaz (yerel geliştirme).
    public static IApplicationBuilder UseClientIpFromHeader(this IApplicationBuilder app, string? headerName)
    {
        if (string.IsNullOrWhiteSpace(headerName))
            return app;

        return app.Use(async (context, next) =>
        {
            var value = context.Request.Headers[headerName].FirstOrDefault();

            if (IPAddress.TryParse(value, out var clientIp))
                context.Connection.RemoteIpAddress = clientIp;

            await next();
        });
    }
}