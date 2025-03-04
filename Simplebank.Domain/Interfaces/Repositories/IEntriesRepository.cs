using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Interfaces.Repositories;

public interface IEntriesRepository : IRepository<Entry>
{
    Task<IEnumerable<Entry>> GetEntriesAsync(Guid accountId, int page, int perPage);
}