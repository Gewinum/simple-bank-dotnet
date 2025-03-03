namespace Simplebank.Domain.Interfaces.Database;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();

    Task CommitTransactionAsync();
    
    Task RollbackTransactionAsync();
}