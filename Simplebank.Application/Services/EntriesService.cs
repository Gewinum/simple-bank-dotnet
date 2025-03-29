using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;

namespace Simplebank.Application.Services;

public class EntriesService : IEntriesService
{
    private readonly IEntriesRepository _entriesRepository;
    private readonly IAccountsRepository _accountsRepository;

    public EntriesService(IEntriesRepository entriesRepository, IAccountsRepository accountsRepository)
    {
        _entriesRepository = entriesRepository;
        _accountsRepository = accountsRepository;
    }
    
    public async Task<IEnumerable<Entry>> GetEntriesAsync(Guid userId, Guid accountId, int page, int perPage)
    {
        var account = await _accountsRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            throw new AccountNotFoundException(accountId);
        }
        
        if (account.OwnerId != userId)
        {
            throw new AccountNotOwnedException(accountId, userId);
        }
        
        return await _entriesRepository.GetEntriesAsync(accountId, page, perPage);
    }
}