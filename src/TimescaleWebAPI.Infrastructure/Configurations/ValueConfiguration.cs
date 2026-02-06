using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimescaleWebAPI.Domain.Entities;

namespace TimescaleWebAPI.Infrastructure.Configurations;

public class ValueConfiguration : IEntityTypeConfiguration<Value>
{
    public void Configure(EntityTypeBuilder<Value> builder)
    {
        builder.ToTable("Values");
        
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.Id)
            .ValueGeneratedOnAdd();
            
        builder.Property(v => v.FileName)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(v => v.Date)
            .IsRequired();
            
        builder.Property(v => v.ExecutionTime)
            .IsRequired()
            .HasColumnType("double precision");
            
        builder.Property(v => v.ValueMetric)
            .IsRequired()
            .HasColumnType("double precision")
            .HasColumnName("Value"); // В базе будет колонка Value
            
        builder.Property(v => v.CreatedAt)
            .IsRequired();
            
        // Значения по умолчанию
        builder.Property(v => v.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
			.ValueGeneratedOnAdd();
    }
}