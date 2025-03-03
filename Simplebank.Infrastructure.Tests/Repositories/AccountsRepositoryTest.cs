using Microsoft.EntityFrameworkCore;
using Moq;
using Simplebank.Domain.Constants;
using Simplebank.Domain.Database.Models;
using Simplebank.Infrastructure.Database;
using Simplebank.Infrastructure.Exceptions;
using Simplebank.Infrastructure.Repositories;

namespace Simplebank.Infrastructure.Tests.Repositories;

public class AccountsRepositoryTest
{
    [Fact]
    public async Task GetAccountByIdAsyncSuccess()
    {
        // Arrange
        var context = InstantiateContext();
        var account = CreateRandomAccount();
        
        await context.AddAsync(account);
        await context.SaveChangesAsync();

        var repository = new AccountsRepository(context);

        // Act
        var result = await repository.GetByIdAsync(account.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.Id, result.Id);
        Assert.Equal(account.Currency, result.Currency);
        Assert.Equal(account.Balance, result.Balance);
    }
    
    [Fact]
    public async Task GetAccountByIdAsyncNotFound()
    {
        // Arrange
        var context = InstantiateContext();
        var repository = new AccountsRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task AddAccountAsyncSuccess()
    {
        // Arrange
        var context = InstantiateContext();
        var account = CreateRandomAccount();
        var repository = new AccountsRepository(context);

        // Act
        await repository.AddAsync(account);

        // Assert
        var result = await repository.GetByIdAsync(account.Id);
        Assert.NotNull(result);
        Assert.Equal(account.Id, result.Id);
        Assert.Equal(account.Currency, result.Currency);
        Assert.Equal(account.Balance, result.Balance);
    }
    
    private ApplicationDbContext InstantiateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestCase")
            .Options;
        return new ApplicationDbContext(options);
    }
    
    private Account CreateRandomAccount()
    {
        var randomCurrency = CurrencyConstants.Currencies[new Random().Next(CurrencyConstants.Currencies.Length)];
        return new Account
        {
            Id = Guid.NewGuid(),
            Owner = RandomString(10),
            Currency = randomCurrency,
            Balance = new Random().Next(0, 1000)
        };
    }

    private string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());
    }
}