namespace Simplebank.Domain.Models.Users;

public class CreateUserRequest
{
    public required string Username { get; set; }
    
    public required string FullName { get; set; }
    
    public required string Email { get; set; }
    
    public required string Password { get; set; }
}