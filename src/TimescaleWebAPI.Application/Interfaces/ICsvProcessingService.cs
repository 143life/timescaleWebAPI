using TimescaleWebAPI.Application.DTOs;
using TimescaleWebAPI.Domain.Entities;

namespace TimescaleWebAPI.Application.Interfaces;

public interface ICsvProcessingService
{
    Task<ProcessingResult> ProcessCsvFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    IEnumerable<CsvRecordDto> ParseCsv(Stream fileStream);
    void ValidateCsvRecords(IEnumerable<CsvRecordDto> records);
    Task<ValueStatistics> CalculateStatisticsAsync(IEnumerable<CsvRecordDto> records, CancellationToken cancellationToken = default);
}

public class ProcessingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProcessedRows { get; set; }
    public IEnumerable<CsvRecordDto> Records { get; set; } = new List<CsvRecordDto>();
}

public class ValueStatistics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double TimeDeltaSeconds { get; set; }
    public double AverageExecutionTime { get; set; }
    public double AverageValue { get; set; }
    public double MedianValue { get; set; }
    public double MaxValue { get; set; }
    public double MinValue { get; set; }
    public int TotalRows { get; set; }
}