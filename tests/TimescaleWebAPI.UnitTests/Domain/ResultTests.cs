using TimescaleWebAPI.Domain.Entities;
using TimescaleWebAPI.Domain.Exceptions;
using Xunit;

namespace TimescaleWebAPI.UnitTests.Domain;

public class ResultTests
{
    [Fact]
    public void Constructor_ShouldCreateResult_WithValidParameters()
    {
        // Arrange
        var fileName = "test.csv";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 2);
        var averageExecutionTime = 1.5;
        var averageValue = 100.0;
        var medianValue = 95.0;
        var maxValue = 150.0;
        var minValue = 50.0;
        var totalRows = 10;

        // Act
        var result = new Result(fileName, startDate, endDate, 
            averageExecutionTime, averageValue, medianValue, 
            maxValue, minValue, totalRows);

        // Assert
        Assert.Equal(fileName, result.FileName);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.Equal((endDate - startDate).TotalSeconds, result.TimeDeltaSeconds);
        Assert.Equal(averageExecutionTime, result.AverageExecutionTime);
        Assert.Equal(averageValue, result.AverageValue);
        Assert.Equal(medianValue, result.MedianValue);
        Assert.Equal(maxValue, result.MaxValue);
        Assert.Equal(minValue, result.MinValue);
        Assert.Equal(totalRows, result.TotalRows);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True((DateTime.UtcNow - result.ProcessedAt).TotalSeconds < 1);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFileNameIsInvalid()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => 
            new Result("", DateTime.UtcNow, DateTime.UtcNow, 1.5, 100.0, 95.0, 150.0, 50.0, 10));
        
        Assert.Throws<DomainException>(() => 
            new Result("   ", DateTime.UtcNow, DateTime.UtcNow, 1.5, 100.0, 95.0, 150.0, 50.0, 10));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenEndDateBeforeStartDate()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 2);
        var endDate = new DateTime(2024, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            new Result("test.csv", startDate, endDate, 1.5, 100.0, 95.0, 150.0, 50.0, 10));
        
        Assert.Contains("EndDate cannot be earlier than StartDate", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTotalRowsNotPositive()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => 
            new Result("test.csv", DateTime.UtcNow, DateTime.UtcNow, 
                1.5, 100.0, 95.0, 150.0, 50.0, 0));
        
        Assert.Throws<DomainException>(() => 
            new Result("test.csv", DateTime.UtcNow, DateTime.UtcNow, 
                1.5, 100.0, 95.0, 150.0, 50.0, -1));
    }

    [Fact]
    public void Update_ShouldUpdateResult_WhenValid()
    {
        // Arrange
        var original = new Result("test.csv", 
            new DateTime(2024, 1, 1), new DateTime(2024, 1, 2),
            1.5, 100.0, 95.0, 150.0, 50.0, 10);
        
        var updated = new Result("test.csv",
            new DateTime(2024, 1, 3), new DateTime(2024, 1, 4),
            2.0, 120.0, 110.0, 200.0, 80.0, 20);

        // Act
        original.Update(updated);

        // Assert
        Assert.Equal(updated.StartDate, original.StartDate);
        Assert.Equal(updated.EndDate, original.EndDate);
        Assert.Equal(updated.TimeDeltaSeconds, original.TimeDeltaSeconds);
        Assert.Equal(updated.AverageExecutionTime, original.AverageExecutionTime);
        Assert.Equal(updated.AverageValue, original.AverageValue);
        Assert.Equal(updated.MedianValue, original.MedianValue);
        Assert.Equal(updated.MaxValue, original.MaxValue);
        Assert.Equal(updated.MinValue, original.MinValue);
        Assert.Equal(updated.TotalRows, original.TotalRows);
    }

    [Fact]
    public void Update_ShouldThrow_WhenFileNameDifferent()
    {
        // Arrange
        var original = new Result("file1.csv", 
            DateTime.UtcNow, DateTime.UtcNow, 1.5, 100.0, 95.0, 150.0, 50.0, 10);
        
        var updated = new Result("file2.csv",
            DateTime.UtcNow, DateTime.UtcNow, 2.0, 120.0, 110.0, 200.0, 80.0, 20);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => original.Update(updated));
        Assert.Contains("Cannot update result with different file name", exception.Message);
    }
}