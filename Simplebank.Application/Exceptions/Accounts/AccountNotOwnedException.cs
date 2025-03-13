using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Accounts;

public class AccountNotOwnedException : Exception, IIdentifiableException
{
    public AccountNotOwnedException(Guid accountId, Guid userId)
    {
        AccountId = accountId;
        UserId = userId;
    }

    public Guid AccountId { get; }
    
    public Guid UserId { get; }
    
    public override string Message => $"Account with id {AccountId} is not owned by user with id {UserId}";
    
    public string ErrorType => "AccountNotOwned";
}