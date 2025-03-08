using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Interfaces.Repositories;

public interface IAccountsRepository : IRepository<Account>
{
    Task<Account?> GetWithLockAsync(Guid id);
    
    Task<IEnumerable<Account>> GetByOwnerAsync(string owner);
    
    Task<Account?> AddBalanceAsync(Guid id, decimal amount);
}