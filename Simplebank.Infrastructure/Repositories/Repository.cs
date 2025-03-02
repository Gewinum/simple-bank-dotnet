using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Simplebank.Domain.Interfaces.Models;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Infrastructure.Database;

namespace Simplebank.Infrastructure.Repositories;

public abstract class Repository<T>(ApplicationDbContext context) : IRepository<T>
    where T : class
{ 
    protected readonly DbSet<T> dbSet = context.Set<T>();
    
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await dbSet.FindAsync(id);
    }

    public async Task<EntityEntry<T>?> AddAsync(T entity)
    {
        if (entity is IModel model)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
        }
        
        var entityEntry = await dbSet.AddAsync(entity);
        return entityEntry;
    }
    
    public void Update(T entity)
    {
        if (entity is IModel model)
        {
            model.UpdatedAt = DateTime.UtcNow;
        }
        
        dbSet.Update(entity);
    }

    public EntityEntry<T>? Delete(T entity)
    {
        return dbSet.Remove(entity);
    }
    
    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
    
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await context.Database.BeginTransactionAsync();
    }
}