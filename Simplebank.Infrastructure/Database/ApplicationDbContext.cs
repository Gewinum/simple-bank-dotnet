using Microsoft.EntityFrameworkCore;
using Simplebank.Domain.Database.Models;

namespace Simplebank.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; }
    
    public DbSet<Entry> Entries { get; set; }
    
    public DbSet<Transfer> Transfers { get; set; }

    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .HasIndex(a => new { a.OwnerId, a.Currency })
            .HasDatabaseName("IX_Accounts_Owner_Currency")
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .HasDatabaseName("IX_Users_Login")
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .HasDatabaseName("IX_Users_Email")
            .IsUnique();
    }
}