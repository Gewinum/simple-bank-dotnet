using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Interfaces.Services;

public interface IAccountsService
{
    Task<Account> GetAccountAsync(Guid id);
    
    Task<Account?> CreateAccountAsync(string owner, string currency);
    
    Task AddBalanceAsync(Guid id, decimal amount);
    
    Task SubtractBalanceAsync(Guid id, decimal amount);
}