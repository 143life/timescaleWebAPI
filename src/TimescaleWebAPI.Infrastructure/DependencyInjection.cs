using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimescaleWebAPI.Domain.Interfaces;
using TimescaleWebAPI.Infrastructure.Data;
using TimescaleWebAPI.Infrastructure.Repositories;

namespace TimescaleWebAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        // Регистрируем репозитории
        services.AddScoped<IValueRepository, ValueRepository>();
        services.AddScoped<IResultRepository, ResultRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}