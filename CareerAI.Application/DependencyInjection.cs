using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

using CareerAI.Application.Interfaces;
using CareerAI.Application.Services;
namespace CareerAI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Validators klasöründeki tüm AbstractValidator<T> sınıflarını otomatik kaydeder
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        // Durumsuz (stateless) saf mantık: tek örnek yeterli
        services.AddSingleton<IMatchingService, MatchingService>();

        return services;
    }
}