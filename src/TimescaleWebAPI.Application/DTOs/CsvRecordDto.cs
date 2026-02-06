namespace TimescaleWebAPI.Application.DTOs;

public class CsvRecordDto
{
    public DateTime Date { get; set; }
    public double ExecutionTime { get; set; }
    public double Value { get; set; }
}