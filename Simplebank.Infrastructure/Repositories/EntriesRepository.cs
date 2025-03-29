using Microsoft.EntityFrameworkCore;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Infrastructure.Database;

namespace Simplebank.Infrastructure.Repositories;

public class EntriesRepository : Repository<Entry>, IEntriesRepository
{
    public EntriesRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Entry>> GetEntriesAsync(Guid accountId, int page, int perPage)
    {
        return await dbSet.Where(a => a.AccountId == accountId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();
    }
}