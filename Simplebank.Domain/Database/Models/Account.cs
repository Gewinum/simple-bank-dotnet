using System.ComponentModel.DataAnnotations;
using Simplebank.Domain.Attributes;
using Simplebank.Domain.Constants;

namespace Simplebank.Domain.Database.Models;

public class Account : BaseModel
{
    [MaxLength(DatabaseConstants.DefaultStringLength)]
    public required Guid OwnerId { get; set; }
    
    [Decimal(18, 2)]
    public required decimal Balance { get; set; }
    
    [MaxLength(DatabaseConstants.DefaultStringLength)]
    public required string Currency { get; set; }
}