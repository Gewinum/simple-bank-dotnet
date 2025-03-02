namespace Simplebank.Application.Exceptions.Accounts;

public class AccountNotFoundException : Exception
{
    public AccountNotFoundException(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
    
    public override string Message => $"Account with id {Id} was not found";
}