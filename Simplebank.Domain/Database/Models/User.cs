using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Simplebank.Domain.Constants;

namespace Simplebank.Domain.Database.Models;

public class User : BaseModel
{
    [MaxLength(DatabaseConstants.DefaultStringLength)]
    public required string Login { get; set; }
    
    [MaxLength(DatabaseConstants.DefaultStringLength)]
    public required string Name { get; set; }
    
    [MaxLength(DatabaseConstants.DefaultStringLength)]
    public required string Email { get; set; }
    
    [Description("Password must be stored in hashed format")]
    [MaxLength(DatabaseConstants.DefaultStringLength)]
    public required string Password { get; set; }
}