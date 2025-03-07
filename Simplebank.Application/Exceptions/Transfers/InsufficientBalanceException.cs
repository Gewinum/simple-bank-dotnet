namespace Simplebank.Application.Exceptions.Transfers;

public class InsufficientBalanceException : Exception
{
    public InsufficientBalanceException(Guid accountId, decimal amount)
    {
        AccountId = accountId;
        Amount = amount;
    }
    
    public Guid AccountId { get; }
    
    public decimal Amount { get; }
    
    public override string Message => $"Account with id {AccountId} has insufficient balance to transfer {Amount}";
}