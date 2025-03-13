using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Interfaces.Services;

public interface IEntriesService
{
    Task <IEnumerable<Entry>> GetEntriesAsync(Guid userId, Guid accountId, int page, int perPage);
}