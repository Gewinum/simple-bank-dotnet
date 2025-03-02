using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Simplebank.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<EntityEntry<T>?> AddAsync(T entity);
    void Update(T entity);
    EntityEntry<T>? Delete(T entity);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}