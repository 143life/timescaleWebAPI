namespace TimescaleWebAPI.Application.DTOs;

public class ResultFilterDto
{
    public string? FileName { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public double? AverageValueFrom { get; set; }
    public double? AverageValueTo { get; set; }
    public double? AverageExecutionTimeFrom { get; set; }
    public double? AverageExecutionTimeTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}