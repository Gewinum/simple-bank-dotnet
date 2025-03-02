namespace Simplebank.Domain.Models.Users;

public class UserDto
{
    public required int Id { get; set; }
    
    public required string Username { get; set; }
    
    public required string FullName { get; set; }
    
    public required string Email { get; set; }
    
    public required string Password { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}