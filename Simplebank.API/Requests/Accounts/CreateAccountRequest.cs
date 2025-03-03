using System.ComponentModel.DataAnnotations;
using Simplebank.Domain.Attributes;

namespace Simplebank.API.Requests.Accounts;

public class CreateAccountRequest
{
    [MinLength(3)]
    [MaxLength(100)]
    public required string Owner { get; set; }
    
    [Currency]
    public required string Currency { get; set; }
}