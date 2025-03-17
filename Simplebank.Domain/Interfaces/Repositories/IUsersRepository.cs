using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Interfaces.Repositories;

public interface IUsersRepository : IRepository<User>
{
    Task<User?> GetByLoginAsync(string login);
    
    Task<User?> GetByEmailAsync(string email);
}