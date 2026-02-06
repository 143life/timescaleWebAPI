using Microsoft.EntityFrameworkCore;
using TimescaleWebAPI.Domain.Entities;
using TimescaleWebAPI.Domain.Interfaces;
using TimescaleWebAPI.Infrastructure.Data;

namespace TimescaleWebAPI.Infrastructure.Repositories;

public class ValueRepository : BaseRepository<Value>, IValueRepository
{
    public ValueRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(v => v.FileName == fileName, cancellationToken);
    }

    public async Task DeleteByFileNameAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var values = await _dbSet
            .Where(v => v.FileName == fileName)
            .ToListAsync(cancellationToken);

        if (values.Any())
        {
            _dbSet.RemoveRange(values);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task BulkInsertAsync(IEnumerable<Value> values, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(values, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Value>> GetLatestValuesAsync(
        string fileName, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.FileName == fileName)
            .OrderByDescending(v => v.Date)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Value>> GetValuesByFileNameAsync(
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.FileName == fileName)
            .ToListAsync(cancellationToken);
    }
}