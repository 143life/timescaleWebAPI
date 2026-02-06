using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TimescaleWebAPI.Application.DTOs;
using TimescaleWebAPI.Application.Extensions;
using TimescaleWebAPI.Application.Interfaces;
using TimescaleWebAPI.Application.Validators;

namespace TimescaleWebAPI.Application.Services;

public class CsvProcessingService : ICsvProcessingService
{
    private readonly ILogger<CsvProcessingService> _logger;
    private readonly CsvValidator _validator;

    public CsvProcessingService(ILogger<CsvProcessingService> logger, CsvValidator validator)
    {
        _logger = logger;
        _validator = validator;
    }

    public async Task<ProcessingResult> ProcessCsvFileAsync(
        Stream fileStream, 
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var records = ParseCsv(fileStream).ToList();
            _validator.ValidateRecords(records);
            
            return new ProcessingResult
            {
                Success = true,
                Message = $"Successfully processed {records.Count} rows from file '{fileName}'",
                ProcessedRows = records.Count,
                Records = records
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CSV file '{FileName}'", fileName);
            return new ProcessingResult
            {
                Success = false,
                Message = $"Error processing CSV: {ex.Message}",
                ProcessedRows = 0
            };
        }
    }

    public IEnumerable<CsvRecordDto> ParseCsv(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = context =>
            {
                _logger.LogWarning("Bad data found at row {Row}: {RawRecord}", context.Context.Parser.Row, context.RawRecord);
            }
        };
        
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<CsvRecordMap>();
        
        var records = new List<CsvRecordDto>();
        
        try
        {
            records = csv.GetRecords<CsvRecordDto>().ToList();
        }
        catch (CsvHelperException ex)
        {
            _logger.LogError(ex, "Error parsing CSV");
            throw new FormatException($"Error parsing CSV: {ex.Message}");
        }

        return records;
    }

    public void ValidateCsvRecords(IEnumerable<CsvRecordDto> records)
    {
        _validator.ValidateRecords(records);
    }

    public async Task<ValueStatistics> CalculateStatisticsAsync(
        IEnumerable<CsvRecordDto> records, 
        CancellationToken cancellationToken = default)
    {
        var recordsList = records.ToList();
        
        if (!recordsList.Any())
        {
            throw new ArgumentException("No records provided for statistics calculation");
        }

        var values = recordsList.Select(r => r.Value).ToList();
        
        return new ValueStatistics
        {
            StartDate = recordsList.Min(r => r.Date),
            EndDate = recordsList.Max(r => r.Date),
            TimeDeltaSeconds = (recordsList.Max(r => r.Date) - recordsList.Min(r => r.Date)).TotalSeconds,
            AverageExecutionTime = recordsList.Average(r => r.ExecutionTime),
            AverageValue = recordsList.Average(r => r.Value),
            MedianValue = values.CalculateMedian(),
            MaxValue = recordsList.Max(r => r.Value),
            MinValue = recordsList.Min(r => r.Value),
            TotalRows = recordsList.Count
        };
    }
}

public sealed class CsvRecordMap : ClassMap<CsvRecordDto>
{
    public CsvRecordMap()
    {
        Map(m => m.Date)
            .Name("Date")
            .TypeConverter<CustomDateTimeConverter>();
        
        Map(m => m.ExecutionTime)
            .Name("ExecutionTime");
        
        Map(m => m.Value)
            .Name("Value");
    }
}