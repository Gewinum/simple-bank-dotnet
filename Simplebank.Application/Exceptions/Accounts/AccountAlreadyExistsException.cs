using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Accounts;

public class AccountAlreadyExistsException : Exception, IIdentifiableException
{
    public AccountAlreadyExistsException(Guid ownerId, string currency)
    {
        OwnerId = ownerId;
        Currency = currency;
    }

    public Guid OwnerId { get; }
    
    public string Currency { get; }
    
    public override string Message => $"Account with owner {OwnerId.ToString()} and currency {Currency} already exists";
    
    public string ErrorType => "AccountAlreadyExists";
}