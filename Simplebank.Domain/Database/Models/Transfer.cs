using Simplebank.Domain.Attributes;

namespace Simplebank.Domain.Database.Models;

public class Transfer : BaseModel
{
    public required Guid FromAccountId { get; set; }
    
    public required Guid ToAccountId { get; set; }
    
    [Decimal(18, 2)]
    public required decimal Amount { get; set; }
}