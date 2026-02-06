namespace TimescaleWebAPI.Application.DTOs;

public class UploadCsvCommand
{
    public string FileName { get; set; } = null!;
    public Stream FileStream { get; set; } = null!;
    
    public UploadCsvCommand(string fileName, Stream fileStream)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        FileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
    }
}