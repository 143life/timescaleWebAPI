using TimescaleWebAPI.Domain.Entities;

namespace TimescaleWebAPI.Domain.Interfaces;

public interface IValueRepository : IRepository<Value>
{
    Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default);
    Task DeleteByFileNameAsync(string fileName, CancellationToken cancellationToken = default);
    Task BulkInsertAsync(IEnumerable<Value> values, CancellationToken cancellationToken = default);
    Task<IEnumerable<Value>> GetLatestValuesAsync(string fileName, int count = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Value>> GetValuesByFileNameAsync(string fileName, CancellationToken cancellationToken = default);
}

public interface IResultRepository : IRepository<Result>
{
    Task<Result?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default);
    Task UpsertAsync(Result result, CancellationToken cancellationToken = default);
    Task DeleteByFileNameAsync(string fileName, CancellationToken cancellationToken = default);
}