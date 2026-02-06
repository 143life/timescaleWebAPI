using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimescaleWebAPI.Domain.Entities;

namespace TimescaleWebAPI.Infrastructure.Configurations;

public class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        builder.ToTable("Results");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();
            
        builder.Property(r => r.FileName)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(r => r.StartDate)
            .IsRequired();
            
        builder.Property(r => r.EndDate)
            .IsRequired();
            
        builder.Property(r => r.TimeDeltaSeconds)
            .IsRequired()
            .HasColumnType("double precision");
            
        builder.Property(r => r.AverageExecutionTime)
            .IsRequired()
            .HasColumnType("double precision");
            
        builder.Property(r => r.AverageValue)
            .IsRequired()
            .HasColumnType("double precision");
            
        builder.Property(r => r.MedianValue)
            .IsRequired()
            .HasColumnType("double precision");
            
        builder.Property(r => r.MaxValue)
            .IsRequired()
            .HasColumnType("double precision");
            
        builder.Property(r => r.MinValue)
            .IsRequired()
            .HasColumnType("double precision");
            
        builder.Property(r => r.ProcessedAt)
            .IsRequired();
            
        builder.Property(r => r.TotalRows)
            .IsRequired();
            
        // Значения по умолчанию
        builder.Property(r => r.ProcessedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}