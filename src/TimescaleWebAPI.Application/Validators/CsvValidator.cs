using Microsoft.Extensions.Logging;
using TimescaleWebAPI.Application.DTOs;

namespace TimescaleWebAPI.Application.Validators;

public class CsvValidator
{
    private readonly ILogger<CsvValidator> _logger;

    public CsvValidator(ILogger<CsvValidator> logger)
    {
        _logger = logger;
    }

    public void ValidateRecords(IEnumerable<CsvRecordDto> records)
    {
        var recordsList = records.ToList();
        
        if (recordsList.Count < 1)
            throw new ValidationException("CSV file must contain at least 1 row");
            
        if (recordsList.Count > 10000)
            throw new ValidationException($"CSV file contains too many rows: {recordsList.Count}. Maximum allowed is 10,000");

        var now = DateTime.UtcNow;
        var minDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        for (int i = 0; i < recordsList.Count; i++)
        {
            var record = recordsList[i];
            var rowNumber = i + 2;

			if (record.Date == default)
            	throw new ValidationException($"Row {rowNumber}: Date is required");
            if (record.Date > now)
                throw new ValidationException($"Row {rowNumber}: Date cannot be in the future. Date: {record.Date}");
                
            if (record.Date < minDate)
                throw new ValidationException($"Row {rowNumber}: Date cannot be earlier than 01.01.2000. Date: {record.Date}");
                
            if (record.ExecutionTime < 0)
                throw new ValidationException($"Row {rowNumber}: ExecutionTime cannot be less than 0. Value: {record.ExecutionTime}");
                
            if (record.Value < 0)
                throw new ValidationException($"Row {rowNumber}: Value cannot be less than 0. Value: {record.Value}");
        }
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}