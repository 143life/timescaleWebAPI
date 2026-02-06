using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimescaleWebAPI.Application.Interfaces;
using TimescaleWebAPI.Application.Services;
using TimescaleWebAPI.Application.Validators;
using TimescaleWebAPI.Domain.Interfaces;
using TimescaleWebAPI.Infrastructure;
using TimescaleWebAPI.Infrastructure.Repositories;

namespace TimescaleWebAPI.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрируем сервисы Application слоя
        services.AddScoped<ICsvProcessingService, CsvProcessingService>();
        services.AddScoped<CsvValidator>();
        
        return services;
    }
    
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Контроллеры
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.WriteIndented = true;
            });
            
        // API Versioning (опционально)
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
        
        return services;
    }
}