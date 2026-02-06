using Microsoft.EntityFrameworkCore;
using TimescaleWebAPI.Domain.Entities;
using TimescaleWebAPI.Infrastructure.Configurations;

namespace TimescaleWebAPI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Value> Values => Set<Value>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Применяем все конфигурации из сборки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Создаем индексы для производительности
        modelBuilder.Entity<Value>()
            .HasIndex(v => new { v.FileName, v.Date })
            .IsUnique();
            
        modelBuilder.Entity<Value>()
            .HasIndex(v => v.Date);
            
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.FileName)
            .IsUnique();
            
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.StartDate);
            
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.AverageValue);
            
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.AverageExecutionTime);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}