using System.ComponentModel.DataAnnotations;

namespace Simplebank.Domain.Attributes;

public class CurrencyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string currency)
        {
            return new ValidationResult("Currency must be a string");
        }
        
        var allowedCurrencies = new[] {"USD", "EUR", "GBP", "JPY", "CNY"};

        return !allowedCurrencies.Contains(currency) ? new ValidationResult("Currency is not allowed") : ValidationResult.Success;
    }
}