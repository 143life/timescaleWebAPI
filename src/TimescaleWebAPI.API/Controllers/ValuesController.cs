using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimescaleWebAPI.API.DTOs;
using TimescaleWebAPI.Application.DTOs;
using TimescaleWebAPI.Application.Interfaces;
using TimescaleWebAPI.Domain.Entities;
using TimescaleWebAPI.Domain.Interfaces;
using TimescaleWebAPI.Infrastructure.Repositories;

namespace TimescaleWebAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    private readonly ICsvProcessingService _csvProcessingService;
    private readonly IValueRepository _valueRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ValuesController> _logger;

    public ValuesController(
        ICsvProcessingService csvProcessingService,
        IValueRepository valueRepository,
        IResultRepository resultRepository,
        IUnitOfWork unitOfWork,
        ILogger<ValuesController> logger)
    {
        _csvProcessingService = csvProcessingService;
        _valueRepository = valueRepository;
        _resultRepository = resultRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Загружает CSV файл с данными временных рядов
    /// </summary>
    /// <param name="request">Запрос с файлом CSV</param>
    /// <returns>Результат обработки файла со статистикой</returns>
    /// <remarks>
    /// Пример запроса:
    /// POST /api/values/upload
    /// Content-Type: multipart/form-data
    /// 
    /// Формат CSV:
    /// Date;ExecutionTime;Value
    /// 2024-01-01T10-00-00.0000Z;1.5;100.0
    /// </remarks>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadCsv([FromForm] ApiUploadCsvRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "No file uploaded",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var fileName = string.IsNullOrWhiteSpace(request.FileName) 
            ? request.File.FileName 
            : request.FileName;

		// Добавляем расширение .csv если его нет
		if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
		{
			fileName += ".csv";
		}

        // Проверяем расширение ОРИГИНАЛЬНОГО файла
		var originalExtension = Path.GetExtension(request.File.FileName);
		if (!originalExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
		{
			return BadRequest(new ProblemDetails
			{
				Title = "Invalid file type",
				Detail = "Only CSV files are allowed",
				Status = StatusCodes.Status400BadRequest
			});
		}

        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            using var fileStream = request.File.OpenReadStream();
            var command = new UploadCsvCommand(fileName, fileStream);
            
            var processingResult = await _csvProcessingService.ProcessCsvFileAsync(
                command.FileStream, 
                command.FileName);

            if (!processingResult.Success)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest(new ProblemDetails
                {
                    Title = "CSV processing failed",
                    Detail = processingResult.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            // Проверяем, существует ли файл
            bool fileExists = await _valueRepository.FileExistsAsync(fileName);
            
            if (fileExists)
            {
                await _valueRepository.DeleteByFileNameAsync(fileName);
                await _resultRepository.DeleteByFileNameAsync(fileName);
                _logger.LogInformation("Deleted existing data for file '{FileName}'", fileName);
            }

            // Сохраняем значения
            var values = processingResult.Records.Select(record => 
                new Value(fileName, record.Date, record.ExecutionTime, record.Value)
            ).ToList();

            await _valueRepository.BulkInsertAsync(values);

            // Рассчитываем статистику
            var statistics = await _csvProcessingService.CalculateStatisticsAsync(processingResult.Records);

            // Создаем результат
            var result = new Result(
                fileName: fileName,
                startDate: statistics.StartDate,
                endDate: statistics.EndDate,
                averageExecutionTime: statistics.AverageExecutionTime,
                averageValue: statistics.AverageValue,
                medianValue: statistics.MedianValue,
                maxValue: statistics.MaxValue,
                minValue: statistics.MinValue,
                totalRows: statistics.TotalRows
            );

            await _resultRepository.UpsertAsync(result);
            await _unitOfWork.CommitTransactionAsync();

            return Ok(new
            {
                Message = processingResult.Message,
                FileName = fileName,
                TotalRows = processingResult.ProcessedRows,
                Statistics = new
                {
                    statistics.StartDate,
                    statistics.EndDate,
                    statistics.TimeDeltaSeconds,
                    statistics.AverageExecutionTime,
                    statistics.AverageValue,
                    statistics.MedianValue,
                    statistics.MaxValue,
                    statistics.MinValue
                }
            });
        }
        catch (ValidationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return BadRequest(new ProblemDetails
            {
                Title = "Validation error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (FormatException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return BadRequest(new ProblemDetails
            {
                Title = "Format error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error processing file '{FileName}'", fileName);
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while processing the file",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// Получает отфильтрованные результаты обработки файлов
    /// </summary>
    /// <param name="filter">Параметры фильтрации</param>
    /// <returns>Список результатов с пагинацией</returns>
    /// <remarks>
    /// Пример запроса:
    /// GET /api/values/results?fileName=test&amp;startDateFrom=2024-01-01&amp;pageSize=10
    /// </remarks>
    [HttpGet("results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetResults([FromQuery] ResultFilterDto filter)
    {
        try
        {
            if (filter.PageNumber < 1)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid page number",
                    Detail = "PageNumber must be greater than 0",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid page size",
                    Detail = "PageSize must be between 1 and 100",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            // Получаем все результаты
            var allResults = await _resultRepository.GetAllAsync();
            
            // Применяем фильтры в памяти (в реальном проекте лучше делать через спецификации)
            var filtered = allResults.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(filter.FileName))
            {
                filtered = filtered.Where(r => r.FileName.Contains(filter.FileName));
            }

            if (filter.StartDateFrom.HasValue)
            {
                filtered = filtered.Where(r => r.StartDate >= filter.StartDateFrom.Value);
            }

            if (filter.StartDateTo.HasValue)
            {
                filtered = filtered.Where(r => r.StartDate <= filter.StartDateTo.Value);
            }

            if (filter.AverageValueFrom.HasValue)
            {
                filtered = filtered.Where(r => r.AverageValue >= filter.AverageValueFrom.Value);
            }

            if (filter.AverageValueTo.HasValue)
            {
                filtered = filtered.Where(r => r.AverageValue <= filter.AverageValueTo.Value);
            }

            if (filter.AverageExecutionTimeFrom.HasValue)
            {
                filtered = filtered.Where(r => r.AverageExecutionTime >= filter.AverageExecutionTimeFrom.Value);
            }

            if (filter.AverageExecutionTimeTo.HasValue)
            {
                filtered = filtered.Where(r => r.AverageExecutionTime <= filter.AverageExecutionTimeTo.Value);
            }

            // Пагинация
            var total = filtered.Count();
            var results = filtered
                .OrderByDescending(r => r.ProcessedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            // Маппим в DTO
            var resultDtos = results.Select(r => new ResultDto
            {
                Id = r.Id,
                FileName = r.FileName,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TimeDeltaSeconds = r.TimeDeltaSeconds,
                AverageExecutionTime = r.AverageExecutionTime,
                AverageValue = r.AverageValue,
                MedianValue = r.MedianValue,
                MaxValue = r.MaxValue,
                MinValue = r.MinValue,
                ProcessedAt = r.ProcessedAt,
                TotalRows = r.TotalRows
            });

            return Ok(new
            {
                Total = total,
                Page = filter.PageNumber,
                PageSize = filter.PageSize,
                Results = resultDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting results");
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while retrieving results",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// Получает последние 10 значений для указанного файла
    /// </summary>
    /// <param name="fileName">Имя файла (с расширением .csv или без)</param>
    /// <returns>Последние 10 записей отсортированных по дате</returns>
    /// <remarks>
    /// Пример запроса:
    /// GET /api/values/test.csv/latest
    /// </remarks>
    [HttpGet("{fileName}/latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLatestValues(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid file name",
                    Detail = "FileName is required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

			// Добавляем .csv если его нет
			if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
			{
				fileName += ".csv";
			}

            var values = await _valueRepository.GetLatestValuesAsync(fileName, 10);
            
            if (!values.Any())
            {
                return NotFound(new ProblemDetails
                {
                    Title = "File not found",
                    Detail = $"No data found for file '{fileName}'",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var valueDtos = values.Select(v => new ValueDto
            {
                Date = v.Date,
                ExecutionTime = v.ExecutionTime,
                Value = v.ValueMetric
            }).OrderByDescending(v => v.Date);

            return Ok(new
            {
                FileName = fileName,
                Total = valueDtos.Count(),
                Values = valueDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest values for file '{FileName}'", fileName);
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while retrieving values",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }
}