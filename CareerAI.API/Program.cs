using System.Text;
using System.Threading.RateLimiting;
using CareerAI.API.Extensions;
using CareerAI.API.Middleware;
using CareerAI.Application;
using CareerAI.Application.Interfaces;
using CareerAI.Infrastructure;
using CareerAI.Infrastructure.AI;
using CareerAI.Infrastructure.Auth;
using CareerAI.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;

// Bootstrap logger: uygulama ayağa kalkarken (yapılandırma okunurken) oluşan hataları da yakalar
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ---------- Serilog ----------
    // Hassas veri kuralı: istek/yanıt gövdesi, Authorization başlığı, şifre, token, CV metni ve API anahtarı asla loglanmaz.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllers();

    // ---------- Swagger + Bearer ----------
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { Title = "CareerAI API", Version = "v1" });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Login'den dönen accessToken değerini yapıştırın (başına 'Bearer' yazmayın)."
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });

    // ---------- Configuration ----------
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection bulunamadı. User Secrets veya environment variable tanımlayın.");

    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
        ?? throw new InvalidOperationException("'Jwt' yapılandırma bölümü bulunamadı.");

    if (Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
        throw new InvalidOperationException(
            "Jwt:Key en az 32 byte olmalı. User Secrets veya environment variable ile tanımlayın.");

    var storageSettings = builder.Configuration.GetSection(FileStorageSettings.SectionName).Get<FileStorageSettings>()
        ?? new FileStorageSettings();

    // Göreli yolu uygulamanın içerik klasörüne göre mutlak yola çevir
    if (!Path.IsPathRooted(storageSettings.RootPath))
        storageSettings.RootPath = Path.Combine(builder.Environment.ContentRootPath, storageSettings.RootPath);

    var openAiSettings = builder.Configuration.GetSection(OpenAiSettings.SectionName).Get<OpenAiSettings>()
        ?? new OpenAiSettings();

    if (string.IsNullOrWhiteSpace(openAiSettings.ApiKey))
        throw new InvalidOperationException(
            "OpenAI:ApiKey bulunamadı. User Secrets (geliştirme) veya OpenAI__ApiKey environment variable'ı (production) ile tanımlayın.");

    // ---------- Katmanlar ----------
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(connectionString, jwtSettings, storageSettings, openAiSettings);

    // ---------- Authentication ----------
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false; // "sub" claim'i olduğu gibi kalsın
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            options.Events = new JwtBearerEvents
            {
                // İmza ve süre geçerli olsa bile oturum iptal edilmiş olabilir:
                // şifre değişti (stamp yenilendi) ya da hesap silindi. Her istekte veritabanından doğrularız.
                OnTokenValidated = async context =>
                {
                    var userIdClaim = context.Principal?.FindFirst("sub")?.Value;
                    var stampClaim = context.Principal?.FindFirst(AuthClaims.SecurityStamp)?.Value;

                    var validator = context.HttpContext.RequestServices.GetRequiredService<IUserSessionValidator>();

                    var isValid = Guid.TryParse(userIdClaim, out var userId)
                                  && Guid.TryParse(stampClaim, out var stamp)
                                  && await validator.IsSessionValidAsync(userId, stamp, context.HttpContext.RequestAborted);

                    if (!isValid)
                        context.Fail("The session is no longer valid.");
                }
            };
        });
    builder.Services.AddAuthorization();

    // ---------- Hız sınırı (rate limiting) ----------
    var authPermitLimit = builder.Configuration.GetValue("RateLimiting:AuthPermitLimit", 10);
    var aiPermitLimit = builder.Configuration.GetValue("RateLimiting:AiPermitLimit", 10);

    static RateLimitPartition<string> PerMinute(string key, int permitLimit) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // AI çağrıları para harcar: kullanıcı başına sınır
        options.AddPolicy("ai", httpContext =>
        {
            var key = httpContext.User.FindFirst("sub")?.Value
                      ?? httpContext.Connection.RemoteIpAddress?.ToString()
                      ?? "anonymous";

            return PerMinute(key, aiPermitLimit);
        });

        // Giriş/kayıt/şifre doğrulama: IP başına sınır (kaba kuvvet denemelerini yavaşlatır).
        // UYARI: ters vekil (proxy) arkasında tüm kullanıcılar aynı IP'yi görür. Deployment'ta ForwardedHeaders yapılandırılmalı.
        options.AddPolicy("auth", httpContext =>
            PerMinute(httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", authPermitLimit));

        options.OnRejected = async (context, cancellationToken) =>
        {
            var response = context.HttpContext.Response;
            response.StatusCode = StatusCodes.Status429TooManyRequests;

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = "Too many requests. Please wait a moment and try again.",
                Instance = context.HttpContext.Request.Path
            };
            problem.Extensions["code"] = "rate_limited";

            await response.WriteAsJsonAsync(
                problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        };
    });

    // ---------- Hata yönetimi ----------
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ---------- CORS ----------
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    var app = builder.Build();

    // İstek logu hata yakalayıcının DIŞINDA: yönetilen hatalar (400, 404...) gerçek durum koduyla loglanır.
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms (user {UserId})";

        options.GetLevel = (httpContext, _, exception) =>
            exception is not null || httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error
            : httpContext.Response.StatusCode >= 400 ? LogEventLevel.Warning
            : LogEventLevel.Information;

        // Kimlik doğrulama sonrası kullanıcı ID'si (e-posta ya da isim değil)
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "anonymous");
    });

    app.UseExceptionHandler(); // sonraki middleware'lerdeki hataları yakalar
    app.UseApiSecurityHeaders();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options => options.EnablePersistAuthorization());
    }

    app.UseCors("Frontend");
    app.UseAuthentication(); // önce "kimsin?"
    app.UseAuthorization();  // sonra "buna iznin var mı?"
    app.UseRateLimiter();    // kimlik belli olduktan sonra: kullanıcı başına sınır
    app.MapControllers();

    app.Run();
    return 0;
}
catch (Exception ex) when (ex is not HostAbortedException) // dotnet ef araçları host'u bilerek durdurur
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}