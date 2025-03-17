using System.ComponentModel.DataAnnotations;

namespace Simplebank.API.Requests.Users;

public class CreateUserRequest
{
    [MinLength(3)]
    [MaxLength(100)]
    public required string Login { get; set; }
    
    [MinLength(3)]
    [MaxLength(100)]
    public required string Name { get; set; }
    
    [EmailAddress]
    public required string Email { get; set; }
    
    [MinLength(8)]
    [MaxLength(100)]
    public required string Password { get; set; }
}