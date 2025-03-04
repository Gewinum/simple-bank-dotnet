using System.ComponentModel.DataAnnotations;
using Simplebank.Domain.Attributes;

namespace Simplebank.Domain.Database.Models;

public class Entry : BaseModel
{
    public required Guid AccountId { get; set; }

    [Decimal(18, 2)]
    public required decimal Amount { get; set; }
    
    [MaxLength(1000)]
    public required string Description { get; set; }
}