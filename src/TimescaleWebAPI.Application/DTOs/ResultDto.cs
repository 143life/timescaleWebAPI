namespace TimescaleWebAPI.Application.DTOs;

public class ResultDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double TimeDeltaSeconds { get; set; }
    public double AverageExecutionTime { get; set; }
    public double AverageValue { get; set; }
    public double MedianValue { get; set; }
    public double MaxValue { get; set; }
    public double MinValue { get; set; }
    public DateTime ProcessedAt { get; set; }
    public int TotalRows { get; set; }
}