using Microsoft.EntityFrameworkCore;
using Simplebank.Domain.Database.Models;
using Simplebank.Infrastructure.Database;
using Simplebank.Infrastructure.Repositories;

namespace Simplebank.Infrastructure.Tests.Repositories;

public class EntriesRepositoryTest
{
    [Fact]
    public async Task GetEntriesSuccess()
    {
        // Arrange
        var context = InstantiateContext();
        var entries = new List<Entry>();
        var accountId = Guid.NewGuid();
        
        for (var i = 0; i < 10; i++)
        {
            var entry = CreateRandomEntry(accountId);
            entries.Add(entry);
            await context.AddAsync(entry);
        }
        
        await context.SaveChangesAsync();
        
        var repository = new EntriesRepository(context);
        
        // Act
        var result = await repository.GetEntriesAsync(accountId, 1, 5);
        
        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(5, resultList.Count);

        foreach (var entry in resultList)
        {
            Assert.Contains(entry, entries);
        }
    }

    [Fact]
    public async Task GetNoEntries()
    {
        // Arrange
        var context = InstantiateContext();
        
        for (var i = 0; i < 10; i++)
        {
            var entry = CreateRandomEntry(Guid.NewGuid());
            await context.AddAsync(entry);
        }
        
        await context.SaveChangesAsync();
        
        var repository = new EntriesRepository(context);
        
        // Act
        var result = await repository.GetEntriesAsync(Guid.NewGuid(), 1, 5);
        
        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Empty(resultList);
    }
    
    private ApplicationDbContext InstantiateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestCase")
            .Options;
        return new ApplicationDbContext(options);
    }

    private Entry CreateRandomEntry(Guid accountId)
    {
        return new Entry
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = new Random().Next(1000),
            Description = RandomString(10),
            CreatedAt = DateTime.Now.Add(TimeSpan.FromDays(-new Random().Next(1, 100))),
            UpdatedAt = DateTime.Now.Add(TimeSpan.FromDays(-new Random().Next(1, 100))),
        };
    }

    private string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());
    }
}