using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Interfaces.Services;

public interface IAccountsService
{
    Task<Account> GetAccountAsync(Guid userId, Guid id);
    
    Task<IEnumerable<Account>> GetAccountsAsync(Guid userId);
    
    Task<Account?> CreateAccountAsync(Guid ownerId, string currency);
    
    Task<Entry> AddBalanceAsync(Guid userId, Guid id, decimal amount);
}