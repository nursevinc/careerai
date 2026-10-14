namespace CareerAI.API.Extensions;

public static class ApplicationBuilderExtensions
{
    // API yanıtlarına güvenlik başlıkları ekler. CSP ve no-store sadece /api için (Swagger arayüzü etkilenmez).
    public static IApplicationBuilder UseApiSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                headers["X-Content-Type-Options"] = "nosniff";   // tarayıcı içeriği tahmin etmesin
                headers["X-Frame-Options"] = "DENY";             // iframe içine gömülemez (clickjacking)
                headers["Referrer-Policy"] = "no-referrer";

                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    headers["Cache-Control"] = "no-store";       // kişiye özel veri. önbelleğe alınmasın
                    headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
                }

                return Task.CompletedTask;
            });

            await next();
        });
}