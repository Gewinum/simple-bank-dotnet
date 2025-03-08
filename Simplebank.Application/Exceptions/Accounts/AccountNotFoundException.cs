using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Accounts;

public class AccountNotFoundException : Exception, IIdentifiableException
{
    public AccountNotFoundException(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
    
    public override string Message => $"Account with id {Id} was not found";
    
    public string ErrorType => "AccountNotFound";
}