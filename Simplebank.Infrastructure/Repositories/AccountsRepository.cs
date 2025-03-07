using Microsoft.EntityFrameworkCore;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Infrastructure.Database;

namespace Simplebank.Infrastructure.Repositories;

public class AccountsRepository(ApplicationDbContext context) : Repository<Account>(context), IAccountsRepository
{
    public async Task<IEnumerable<Account>> GetByOwnerAsync(string owner)
    {
        return await dbSet.Where(a => a.Owner == owner).ToListAsync();
    }

    public async Task<Account?> AddBalanceAsync(Guid id, decimal amount)
    {
        var result = dbSet
            .FromSqlRaw("UPDATE Accounts SET Balance = Balance + {0} OUTPUT INSERTED.* WHERE Id = {1}", amount, id)
            .AsAsyncEnumerable();
        await using var asyncEnumerator = result.GetAsyncEnumerator();
        var hasResult = await asyncEnumerator.MoveNextAsync();
        return hasResult ? asyncEnumerator.Current : null;
    }
}