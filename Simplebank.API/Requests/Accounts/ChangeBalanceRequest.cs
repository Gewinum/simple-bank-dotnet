using System.ComponentModel.DataAnnotations;

namespace Simplebank.API.Requests.Accounts;

public class ChangeBalanceRequest
{
    public required Guid AccountId { get; set; }
    
    [Range(-100000.00, 1000000.00)]
    [RegularExpression(@"^-?((\d+\.\d{1,2})|\d+)$", ErrorMessage = "Specified decimal is incorrect")]
    public required decimal Amount { get; set; }
}