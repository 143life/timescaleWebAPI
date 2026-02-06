using TimescaleWebAPI.Domain.Entities;
using TimescaleWebAPI.Domain.Exceptions;
using Xunit;

namespace TimescaleWebAPI.UnitTests.Domain;

public class ValueTests
{
    [Fact]
    public void Constructor_ShouldCreateValue_WithValidParameters()
    {
        // Arrange
        var fileName = "test.csv";
        var date = new DateTime(2024, 1, 1);
        var executionTime = 1.5;
        var valueMetric = 100.0;

        // Act
        var value = new Value(fileName, date, executionTime, valueMetric);

        // Assert
        Assert.Equal(fileName, value.FileName);
        Assert.Equal(date, value.Date);
        Assert.Equal(executionTime, value.ExecutionTime);
        Assert.Equal(valueMetric, value.ValueMetric);
        Assert.NotEqual(Guid.Empty, value.Id);
        Assert.True((DateTime.UtcNow - value.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFileNameIsNullOrEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Value(null, DateTime.UtcNow, 1.5, 100.0));
        
        Assert.Throws<DomainException>(() => 
            new Value("", DateTime.UtcNow, 1.5, 100.0));
        
        Assert.Throws<DomainException>(() => 
            new Value("   ", DateTime.UtcNow, 1.5, 100.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDateIsBefore2000()
    {
        // Arrange
        var oldDate = new DateTime(1999, 12, 31);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Value("test.csv", oldDate, 1.5, 100.0));
        
        Assert.Contains("cannot be earlier than 2000-01-01", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDateIsInFuture()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Value("test.csv", futureDate, 1.5, 100.0));
        
        Assert.Contains("cannot be in the future", exception.Message);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-0.1)]
    public void Constructor_ShouldThrow_WhenExecutionTimeNegative(double executionTime)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Value("test.csv", new DateTime(2024, 1, 1), executionTime, 100.0));
        
        Assert.Contains("ExecutionTime cannot be negative", exception.Message);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-0.1)]
    public void Constructor_ShouldThrow_WhenValueMetricNegative(double valueMetric)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Value("test.csv", new DateTime(2024, 1, 1), 1.5, valueMetric));
        
        Assert.Contains("Value cannot be negative", exception.Message);
    }

    [Fact]
    public void UpdateFileName_ShouldUpdate_WhenValidFileName()
    {
        // Arrange
        var value = new Value("old.csv", new DateTime(2024, 1, 1), 1.5, 100.0);
        var newFileName = "new.csv";

        // Act
        value.UpdateFileName(newFileName);

        // Assert
        Assert.Equal(newFileName, value.FileName);
    }

    [Fact]
    public void UpdateFileName_ShouldThrow_WhenFileNameIsInvalid()
    {
        // Arrange
        var value = new Value("test.csv", new DateTime(2024, 1, 1), 1.5, 100.0);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => value.UpdateFileName(null));
        Assert.Throws<DomainException>(() => value.UpdateFileName(""));
        Assert.Throws<DomainException>(() => value.UpdateFileName("   "));
    }
}