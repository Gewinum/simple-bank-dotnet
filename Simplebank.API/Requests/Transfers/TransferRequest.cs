using System.ComponentModel.DataAnnotations;

namespace Simplebank.API.Requests.Transfers;

public class TransferRequest
{
    public required Guid FromAccount { get; set; }
    
    public required Guid ToAccount { get; set; }
    
    [Range(-100000.00, 1000000.00)]
    [RegularExpression(@"^-?((\d+\.\d{1,2})|\d+)$", ErrorMessage = "Specified decimal is incorrect")]
    public decimal Amount { get; set; }
}