using Microsoft.EntityFrameworkCore;
using TimescaleWebAPI.Domain.Entities;
using TimescaleWebAPI.Infrastructure.Data;
using TimescaleWebAPI.Infrastructure.Repositories;
using Xunit;

namespace TimescaleWebAPI.UnitTests.Infrastructure.Repositories;

public class ValueRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ValueRepository _repository;

    public ValueRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _repository = new ValueRepository(_context);
        
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task BulkInsertAsync_ShouldInsertValues()
    {
        // Arrange
        var values = new List<Value>
        {
            new("test1.csv", new DateTime(2024, 1, 1), 1.0, 100.0),
            new("test2.csv", new DateTime(2024, 1, 2), 2.0, 200.0)
        };

        // Act
        await _repository.BulkInsertAsync(values);

        // Assert
        var savedValues = await _context.Values.ToListAsync();
        Assert.Equal(2, savedValues.Count);
    }

    [Fact]
    public async Task GetLatestValuesAsync_ShouldReturnLatestValues()
    {
        // Arrange
        var fileName = "test.csv";
        var values = new List<Value>
        {
            new(fileName, new DateTime(2024, 1, 1), 1.0, 100.0),
            new(fileName, new DateTime(2024, 1, 2), 2.0, 200.0),
            new(fileName, new DateTime(2024, 1, 3), 3.0, 300.0)
        };
        
        await _repository.BulkInsertAsync(values);

        // Act
        var latest = await _repository.GetLatestValuesAsync(fileName, 2);

        // Assert
        Assert.Equal(2, latest.Count());
        Assert.Equal(new DateTime(2024, 1, 3), latest.First().Date);
        Assert.Equal(new DateTime(2024, 1, 2), latest.Last().Date);
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnTrue_WhenFileExists()
    {
        // Arrange
        var fileName = "test.csv";
        var value = new Value(fileName, new DateTime(2024, 1, 1), 1.0, 100.0);
        
        await _repository.BulkInsertAsync(new[] { value });

        // Act
        var exists = await _repository.FileExistsAsync(fileName);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnFalse_WhenFileNotExists()
    {
        // Act
        var exists = await _repository.FileExistsAsync("nonexistent.csv");

        // Assert
        Assert.False(exists);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}