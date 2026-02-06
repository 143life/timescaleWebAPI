using Microsoft.Extensions.Logging;
using Moq;
using TimescaleWebAPI.Application.DTOs;
using TimescaleWebAPI.Application.Services;
using TimescaleWebAPI.Application.Validators;
using Xunit;

namespace TimescaleWebAPI.UnitTests.Application;

public class CsvProcessingServiceTests
{
    private readonly Mock<ILogger<CsvProcessingService>> _mockLogger;
    private readonly CsvValidator _validator;
    private readonly CsvProcessingService _service;

    public CsvProcessingServiceTests()
    {
        _mockLogger = new Mock<ILogger<CsvProcessingService>>();
        var validatorLogger = Mock.Of<ILogger<CsvValidator>>();
        _validator = new CsvValidator(validatorLogger);
        _service = new CsvProcessingService(_mockLogger.Object, _validator);
    }

    [Fact]
    public async Task ProcessCsvFileAsync_ShouldReturnSuccess_WhenValidCsv()
    {
        // Arrange
        var csvContent = @"Date;ExecutionTime;Value
2024-01-01T10-00-00.0000Z;1.5;100.0";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));
        var fileName = "test.csv";

        // Act
        var result = await _service.ProcessCsvFileAsync(stream, fileName);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Successfully processed", result.Message);
        Assert.Equal(1, result.ProcessedRows);
        Assert.Single(result.Records);
    }

    [Fact]
    public async Task ProcessCsvFileAsync_ShouldReturnFailure_WhenInvalidCsv()
    {
        // Arrange
        var invalidCsv = @"Date;ExecutionTime;Value
invalid-date;1.5;100.0"; // Неправильная дата
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidCsv));
        var fileName = "test.csv";

        // Act
        var result = await _service.ProcessCsvFileAsync(stream, fileName);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Error processing CSV", result.Message);
        Assert.Equal(0, result.ProcessedRows);
        Assert.Empty(result.Records);
    }

    [Fact]
    public void ParseCsv_ShouldParseValidCsv()
    {
        // Arrange
        var csvContent = @"Date;ExecutionTime;Value
2024-01-01T10-00-00.0000Z;1.5;100.0
2024-01-01T10-01-00.0000Z;2.0;150.0";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent));

        // Act
        var records = _service.ParseCsv(stream).ToList();

        // Assert
        Assert.Equal(2, records.Count);
    }

    [Fact]
    public async Task CalculateStatisticsAsync_ShouldCalculateCorrectly()
    {
        // Arrange
        var records = new List<CsvRecordDto>
        {
            new() { Date = new DateTime(2024, 1, 1, 10, 0, 0), ExecutionTime = 1.0, Value = 100.0 },
            new() { Date = new DateTime(2024, 1, 1, 10, 1, 0), ExecutionTime = 2.0, Value = 200.0 },
            new() { Date = new DateTime(2024, 1, 1, 10, 2, 0), ExecutionTime = 3.0, Value = 300.0 }
        };

        // Act
        var statistics = await _service.CalculateStatisticsAsync(records);

        // Assert
        Assert.Equal(120.0, statistics.TimeDeltaSeconds);
        Assert.Equal(2.0, statistics.AverageExecutionTime);
        Assert.Equal(200.0, statistics.AverageValue);
        Assert.Equal(200.0, statistics.MedianValue);
        Assert.Equal(300.0, statistics.MaxValue);
        Assert.Equal(100.0, statistics.MinValue);
        Assert.Equal(3, statistics.TotalRows);
    }

    [Fact]
    public async Task CalculateStatisticsAsync_ShouldThrow_WhenNoRecords()
    {
        // Arrange
        var emptyRecords = new List<CsvRecordDto>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CalculateStatisticsAsync(emptyRecords));
    }
}