using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Simplebank.Domain.Interfaces.Models;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Infrastructure.Database;
using Simplebank.Infrastructure.Exceptions;

namespace Simplebank.Infrastructure.Repositories;

public abstract class Repository<T>(ApplicationDbContext context) : IRepository<T>
    where T : class
{ 
    protected readonly DbSet<T> dbSet = context.Set<T>();
    
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await dbSet.FindAsync(id);
    }

    public async Task<T?> AddAsync(T entity)
    {
        if (entity is IModel model)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
        }

        await dbSet.AddAsync(entity);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is not SqlException innerException)
            {
                throw e.InnerException ?? e;
            }
            
            if (innerException.Number == 2601)
            {
                throw new DuplicateKeysException();
            }

            throw e.InnerException ?? e;
        }
        return entity;
    }
    
    public async Task UpdateAsync(T entity)
    {
        if (entity is IModel model)
        {
            model.UpdatedAt = DateTime.UtcNow;
        }
        
        dbSet.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        dbSet.Remove(entity);
        await context.SaveChangesAsync();
    }
}