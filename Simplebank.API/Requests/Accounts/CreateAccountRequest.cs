using System.ComponentModel.DataAnnotations;

namespace Simplebank.API.Requests.Accounts;

public class CreateAccountRequest
{
    public required string Owner { get; set; }
    
    public required string Currency { get; set; }
}