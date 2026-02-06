using Microsoft.Extensions.Logging;
using Moq;
using TimescaleWebAPI.Application.DTOs;
using TimescaleWebAPI.Application.Validators;
using Xunit;

namespace TimescaleWebAPI.UnitTests.Validators;

public class CsvValidatorTests
{
    private readonly Mock<ILogger<CsvValidator>> _mockLogger;
    private readonly CsvValidator _validator;

    public CsvValidatorTests()
    {
        _mockLogger = new Mock<ILogger<CsvValidator>>();
        _validator = new CsvValidator(_mockLogger.Object);
    }

    [Fact]
    public void ValidateRecords_ShouldThrow_WhenNoRecords()
    {
        // Arrange
        var records = new List<CsvRecordDto>();

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _validator.ValidateRecords(records));
        
        Assert.Contains("CSV file must contain at least 1 row", exception.Message);
    }

    [Fact]
    public void ValidateRecords_ShouldThrow_WhenTooManyRecords()
    {
        // Arrange
        var records = Enumerable.Range(1, 10001)
            .Select(i => new CsvRecordDto
            {
                Date = DateTime.UtcNow.AddDays(-i),
                ExecutionTime = 1.0,
                Value = 100.0
            })
            .ToList();

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _validator.ValidateRecords(records));
        
        Assert.Contains("Maximum allowed is 10,000", exception.Message);
    }

    [Fact]
    public void ValidateRecords_ShouldThrow_WhenExecutionTimeNegative()
    {
        // Arrange
        var records = new List<CsvRecordDto>
        {
            new() { Date = DateTime.UtcNow.AddDays(-1), ExecutionTime = -1.0, Value = 100.0 }
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _validator.ValidateRecords(records));
        
        Assert.Contains("ExecutionTime cannot be less than 0", exception.Message);
    }

    [Fact]
    public void ValidateRecords_ShouldThrow_WhenValueNegative()
    {
        // Arrange
        var records = new List<CsvRecordDto>
        {
            new() { Date = DateTime.UtcNow.AddDays(-1), ExecutionTime = 1.0, Value = -100.0 }
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _validator.ValidateRecords(records));
        
        Assert.Contains("Value cannot be less than 0", exception.Message);
    }

    [Fact]
    public void ValidateRecords_ShouldThrow_WhenDateInFuture()
    {
        // Arrange
        var records = new List<CsvRecordDto>
        {
            new() { Date = DateTime.UtcNow.AddDays(1), ExecutionTime = 1.0, Value = 100.0 }
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _validator.ValidateRecords(records));
        
        Assert.Contains("Date cannot be in the future", exception.Message);
    }

    [Fact]
    public void ValidateRecords_ShouldThrow_WhenDateBefore2000()
    {
        // Arrange
        var records = new List<CsvRecordDto>
        {
            new() { Date = new DateTime(1999, 12, 31), ExecutionTime = 1.0, Value = 100.0 }
        };

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            _validator.ValidateRecords(records));
        
        Assert.Contains("Date cannot be earlier than 01.01.2000", exception.Message);
    }

    [Fact]
    public void ValidateRecords_ShouldPass_WhenValidRecords()
    {
        // Arrange
        var records = new List<CsvRecordDto>
        {
            new() { Date = new DateTime(2024, 1, 1), ExecutionTime = 1.5, Value = 100.0 },
            new() { Date = new DateTime(2024, 1, 2), ExecutionTime = 2.0, Value = 150.0 }
        };

        // Act
        var exception = Record.Exception(() => _validator.ValidateRecords(records));

        // Assert
        Assert.Null(exception);
    }
}