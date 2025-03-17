using Simplebank.Domain.Models.Users;

namespace Simplebank.Domain.Interfaces.Services;

public interface IUsersService
{
    Task<UserDto> GetByIdAsync(Guid id);
    
    Task<UserDto> CreateAsync(string login, string name, string email, string password);
    
    Task<LoginResult> LoginAsync(string login, string password);
}