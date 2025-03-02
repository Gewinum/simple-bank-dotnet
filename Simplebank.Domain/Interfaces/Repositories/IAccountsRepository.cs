using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Interfaces.Repositories;

public interface IAccountsRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetByOwnerAsync(string owner);
}