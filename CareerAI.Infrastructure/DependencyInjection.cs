using CareerAI.Application.Interfaces;
using CareerAI.Infrastructure.AI;
using CareerAI.Infrastructure.Auth;
using CareerAI.Infrastructure.Data;
using CareerAI.Infrastructure.PDF;
using CareerAI.Infrastructure.Services;
using CareerAI.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using CareerAI.Application.Interfaces;
using CareerAI.Infrastructure.AI;
using CareerAI.Infrastructure.Auth;
using CareerAI.Infrastructure.Data;
using CareerAI.Infrastructure.PDF;
using CareerAI.Infrastructure.Services;
using CareerAI.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareerAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        JwtSettings jwtSettings,
        FileStorageSettings storageSettings,
        OpenAiSettings openAiSettings)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        // Auth
        services.AddSingleton(jwtSettings);
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserSessionValidator, UserSessionValidator>();
        services.AddScoped<IAccountService, AccountService>();

        // Resumes
        services.AddSingleton(storageSettings);
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();
        services.AddScoped<IResumeService, ResumeService>();

        // Job applications
        services.AddScoped<IJobApplicationService, JobApplicationService>();

        // AI
        services.AddSingleton(openAiSettings);
        services.AddSingleton<OpenAiJsonClient>();
        services.AddSingleton<ISkillExtractor, OpenAiSkillExtractor>();
        services.AddSingleton<IRecommendationGenerator, OpenAiRecommendationGenerator>();
        services.AddScoped<SkillCatalog>();
        services.AddScoped<ISkillExtractionService, SkillExtractionService>();

        // Analysis & dashboard
        services.AddScoped<IAnalysisService, AnalysisService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}