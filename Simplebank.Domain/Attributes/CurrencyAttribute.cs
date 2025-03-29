using System.ComponentModel.DataAnnotations;
using Simplebank.Domain.Constants;

namespace Simplebank.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class CurrencyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string currency)
        {
            return new ValidationResult("Currency must be a string");
        }
        
        return !CurrencyConstants.Currencies.Contains(currency) ? new ValidationResult("Currency is not allowed") : ValidationResult.Success;
    }
}