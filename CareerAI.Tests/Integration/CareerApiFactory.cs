using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Interfaces;
using CareerAI.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CareerAI.Tests.Integration;

// Gerçek uygulamayı bellek içi bir test sunucusunda çalıştırır.
// Gerçek: HTTP hattı, middleware, JWT, EF Core, SQL Server, PDF okuma. Sahte: sadece OpenAI.
public sealed class CareerApiFactory : WebApplicationFactory<Program>
{
    private const string DatabaseSuffix = "_IntegrationTests";

    // CI ya da başka bir makinede CAREERAI_TEST_DB ortam değişkeniyle değiştirilebilir
    private const string DefaultConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=CareerAI_IntegrationTests;Trusted_Connection=True;TrustServerCertificate=True";

    public string UploadsPath { get; } =
        Path.Combine(Path.GetTempPath(), "careerai-tests-" + Guid.NewGuid().ToString("N"));

    public FakeSkillExtractor SkillExtractor => Services.GetRequiredService<FakeSkillExtractor>();
    public FakeRecommendationGenerator Recommendations => Services.GetRequiredService<FakeRecommendationGenerator>();

    public CareerApiFactory()
    {
        var connectionString = Environment.GetEnvironmentVariable("CAREERAI_TEST_DB") ?? DefaultConnectionString;

        // Güvenlik: testler yanlışlıkla gerçek geliştirme veritabanına bağlanmasın (ve onu silmesin)
        var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        if (!databaseName.EndsWith(DatabaseSuffix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Güvenlik kontrolü: test veritabanının adı '{DatabaseSuffix}' ile bitmeli (şu an: '{databaseName}').");
        }

        // Ortam değişkenleri appsettings.json ve User Secrets'ı ezer: testler her makinede aynı yapılandırmayla çalışır
        Set("ConnectionStrings__DefaultConnection", connectionString);
        Set("Jwt__Key", "integration-tests-signing-key-with-more-than-32-bytes");
        Set("OpenAI__ApiKey", "test-key-never-used");
        Set("RateLimiting__AuthPermitLimit", "1000");
        Set("RateLimiting__AiPermitLimit", "1000");
        Set("Storage__RootPath", UploadsPath);

        // Her test çalıştırmasında temiz bir veritabanı: gerçek migration'larla kurulur (migration'lar da test edilmiş olur)
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.Migrate();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Development" DEĞİL: User Secrets yüklenmesin, geliştirici makinesindeki gerçek anahtarlar testlere sızmasın
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISkillExtractor>();
            services.RemoveAll<IRecommendationGenerator>();

            services.AddSingleton<FakeSkillExtractor>();
            services.AddSingleton<FakeRecommendationGenerator>();
            services.AddSingleton<ISkillExtractor>(sp => sp.GetRequiredService<FakeSkillExtractor>());
            services.AddSingleton<IRecommendationGenerator>(sp => sp.GetRequiredService<FakeRecommendationGenerator>());
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(UploadsPath))
        {
            try
            {
                Directory.Delete(UploadsPath, recursive: true);
            }
            catch (IOException)
            {
                // Geçici klasör: silinemezse sistem temizler
            }
        }
    }

    private static void Set(string name, string value) => Environment.SetEnvironmentVariable(name, value);
}