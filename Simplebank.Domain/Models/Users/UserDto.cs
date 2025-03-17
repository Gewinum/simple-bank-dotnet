namespace Simplebank.Domain.Models.Users;

public class UserDto
{
    public required Guid Id { get; set; }
    
    public required string Login { get; set; }
    
    public required string Name { get; set; }
    
    public required string Email { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}