using Microsoft.EntityFrameworkCore;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Infrastructure.Database;

namespace Simplebank.Infrastructure.Repositories;

public class UsersRepository : Repository<User>, IUsersRepository
{
    public UsersRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await dbSet.FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}