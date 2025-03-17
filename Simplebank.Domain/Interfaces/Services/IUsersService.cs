using Simplebank.Domain.Models.Users;

namespace Simplebank.Domain.Interfaces.Services;

public interface IUsersService
{
    Task<UserDto> GetByIdAsync(Guid id);
    
    Task<UserDto> CreateAsync(string login, string name, string email, string password);
    
    Task<AuthenticationResult> LoginAsync(string login, string password);
}