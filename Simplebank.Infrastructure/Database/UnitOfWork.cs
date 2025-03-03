using Microsoft.EntityFrameworkCore.Storage;
using Simplebank.Domain.Interfaces.Database;

namespace Simplebank.Infrastructure.Database;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    private IDbContextTransaction? _transaction;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _transaction = null;
    }
    
    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Transaction is not started");
        }
        await _transaction.CommitAsync();
        _transaction.Dispose();
        _transaction = null;
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Transaction is not started");
        }
        await _transaction.RollbackAsync();
        _transaction.Dispose();
        _transaction = null;
    }
}