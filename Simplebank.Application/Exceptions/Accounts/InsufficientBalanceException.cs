namespace Simplebank.Application.Exceptions.Accounts;

public class InsufficientBalanceException : Exception
{
    public InsufficientBalanceException(Guid accountId, decimal amount)
    {
        AccountId = accountId;
        Amount = amount;
    }

    public Guid AccountId { get; }
    
    public decimal Amount { get; }
    
    public override string Message => $"Account with id {AccountId} has insufficient balance to withdraw {Amount}";
}