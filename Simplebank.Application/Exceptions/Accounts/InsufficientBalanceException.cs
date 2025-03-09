using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Accounts;

public class InsufficientBalanceException : Exception, IIdentifiableException
{
    public InsufficientBalanceException(Guid accountId, decimal amount)
    {
        AccountId = accountId;
        Amount = amount;
    }
    
    public Guid AccountId { get; }
    
    public decimal Amount { get; }
    
    public override string Message => $"Account with id {AccountId} has insufficient balance to add {Amount}";
    
    public string ErrorType => "InsufficientBalance";
}