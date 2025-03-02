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
}