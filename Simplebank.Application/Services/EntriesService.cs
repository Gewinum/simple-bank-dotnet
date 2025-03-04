using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;

namespace Simplebank.Application.Services;

public class EntriesService : IEntriesService
{
    private readonly IEntriesRepository _entriesRepository;
    
    public EntriesService(IEntriesRepository entriesRepository)
    {
        _entriesRepository = entriesRepository;
    }
    
    public async Task<IEnumerable<Entry>> GetEntriesAsync(Guid accountId, int page, int perPage)
    {
        return await _entriesRepository.GetEntriesAsync(accountId, page, perPage);
    }
}