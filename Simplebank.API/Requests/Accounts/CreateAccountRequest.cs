using System.ComponentModel.DataAnnotations;
using Simplebank.Domain.Attributes;

namespace Simplebank.API.Requests.Accounts;

public class CreateAccountRequest
{
    [Currency]
    public required string Currency { get; set; }
}