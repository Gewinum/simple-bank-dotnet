namespace Simplebank.Application.Exceptions.Accounts;

public class AccountAlreadyExistsException : Exception
{
    public AccountAlreadyExistsException(string owner, string currency)
    {
        Owner = owner;
        Currency = currency;
    }

    public string Owner { get; }
    
    public string Currency { get; }
    
    public override string Message => $"Account with owner {Owner} and currency {Currency} already exists";
}