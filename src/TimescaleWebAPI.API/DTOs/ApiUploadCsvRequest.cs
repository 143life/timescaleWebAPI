using Microsoft.AspNetCore.Http;

namespace TimescaleWebAPI.API.DTOs;

public class ApiUploadCsvRequest
{
    public IFormFile File { get; set; } = null!;
    public string? FileName { get; set; }
}