using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Transfers;

public class DifferentCurrencyAccountsException : Exception, IIdentifiableException
{
    public DifferentCurrencyAccountsException(string currencyFrom, string currencyTo)
    {
        CurrencyFrom = currencyFrom;
        CurrencyTo = currencyTo;
    }
    
    public string CurrencyFrom { get; }
    
    public string CurrencyTo { get; }
    
    public string ErrorType => "DifferentCurrencyAccounts";
    
    public override string Message => $"Accounts have different currencies: {CurrencyFrom} and {CurrencyTo}";
}