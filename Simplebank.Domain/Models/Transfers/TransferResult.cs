using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Models.Transfers;

public class TransferResult
{
    public required Account FromAccount { get; set; }
    
    public required Account ToAccount { get; set; }
    
    public required decimal Amount { get; set; }
    
    public required Entry FromEntry { get; set; }
    
    public required Entry ToEntry { get; set; }
}