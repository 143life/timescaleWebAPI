using TimescaleWebAPI.Domain.Exceptions;

namespace TimescaleWebAPI.Domain.Entities;

public class Result
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public double TimeDeltaSeconds { get; private set; }
    public double AverageExecutionTime { get; private set; }
    public double AverageValue { get; private set; }
    public double MedianValue { get; private set; }
    public double MaxValue { get; private set; }
    public double MinValue { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public int TotalRows { get; private set; }

    private Result() { }

    public Result(
        string fileName,
        DateTime startDate,
        DateTime endDate,
        double averageExecutionTime,
        double averageValue,
        double medianValue,
        double maxValue,
        double minValue,
        int totalRows)
    {
        Id = Guid.NewGuid();
        FileName = fileName;
        StartDate = startDate;
        EndDate = endDate;
        TimeDeltaSeconds = (endDate - startDate).TotalSeconds;
        AverageExecutionTime = averageExecutionTime;
        AverageValue = averageValue;
        MedianValue = medianValue;
        MaxValue = maxValue;
        MinValue = minValue;
        ProcessedAt = DateTime.UtcNow;
        TotalRows = totalRows;
        
        Validate();
    }

    // Бизнес-метод для обновления
    public void Update(Result other)
    {
        if (FileName != other.FileName)
            throw new DomainException("Cannot update result with different file name");
            
        StartDate = other.StartDate;
        EndDate = other.EndDate;
        TimeDeltaSeconds = other.TimeDeltaSeconds;
        AverageExecutionTime = other.AverageExecutionTime;
        AverageValue = other.AverageValue;
        MedianValue = other.MedianValue;
        MaxValue = other.MaxValue;
        MinValue = other.MinValue;
        ProcessedAt = DateTime.UtcNow;
        TotalRows = other.TotalRows;
        
        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(FileName))
            throw new DomainException("FileName cannot be empty");
            
        if (EndDate < StartDate)
            throw new DomainException("EndDate cannot be earlier than StartDate");
            
        if (TotalRows <= 0)
            throw new DomainException("TotalRows must be positive");
    }
}