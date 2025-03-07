using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Interfaces.Services;

public interface IAccountsService
{
    Task<Account> GetAccountAsync(Guid id);
    
    Task<Account?> CreateAccountAsync(string owner, string currency);
    
    Task<Entry> AddBalanceAsync(Guid id, decimal amount);
}