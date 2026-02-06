using Microsoft.EntityFrameworkCore;
using TimescaleWebAPI.Domain.Entities;
using TimescaleWebAPI.Domain.Interfaces;
using TimescaleWebAPI.Infrastructure.Data;

namespace TimescaleWebAPI.Infrastructure.Repositories;

public class ResultRepository : BaseRepository<Result>, IResultRepository
{
    public ResultRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Result?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.FileName == fileName, cancellationToken);
    }

    public async Task UpsertAsync(Result result, CancellationToken cancellationToken = default)
    {
        var existing = await GetByFileNameAsync(result.FileName, cancellationToken);
        
        if (existing != null)
        {
            // Обновляем существующую запись
            existing.Update(result);
            await UpdateAsync(existing, cancellationToken);
        }
        else
        {
            await AddAsync(result, cancellationToken);
        }
    }

    public async Task DeleteByFileNameAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var result = await GetByFileNameAsync(fileName, cancellationToken);
        if (result != null)
        {
            await DeleteAsync(result, cancellationToken);
        }
    }
}