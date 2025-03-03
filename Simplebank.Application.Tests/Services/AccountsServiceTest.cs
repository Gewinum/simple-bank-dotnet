using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Application.Services;
using Simplebank.Domain.Constants;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Database;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Infrastructure.Database;
using Simplebank.Infrastructure.Exceptions;

namespace Simplebank.Application.Tests.Services;

public class AccountsServiceTest
{
    [Fact]
    public async Task GetAccountByIdSuccessTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        var result = await accountsService.GetAccountAsync(account.Id);
        
        // Assert
        Assert.Equal(account, result);
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
    }
    
    [Fact]
    public async Task GetAccountByIdNotFoundTest()
    {
        // Arrange
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        accountsRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Account);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var accountsService = new AccountsService(accountsRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(() => accountsService.GetAccountAsync(Guid.NewGuid()));
        
        // Assert
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task CreateAccountSuccessTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        account.Balance = 0;
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Account>())).ReturnsAsync(account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await accountsService.CreateAccountAsync(account.Owner, account.Currency);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.Owner, result.Owner);
        Assert.Equal(account.Currency, result.Currency);
        Assert.Equal(0, result.Balance);
        accountsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAccountDuplicateTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Account>())).ThrowsAsync(new DuplicateKeysException());
        var accountsService = new AccountsService(accountsRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<AccountAlreadyExistsException>(() => accountsService.CreateAccountAsync(account.Owner, account.Currency));

        // Assert
        Assert.Equal(exception.Owner, account.Owner);
        Assert.Equal(exception.Currency, account.Currency);
        accountsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task AddBalanceSuccessTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        var initialBalance = account.Balance;
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        await accountsService.AddBalanceAsync(account.Id, 10);
        
        // Assert
        Assert.Equal(initialBalance + 10, account.Balance);
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
        accountsRepositoryMock.Verify(r => r.UpdateAsync(account), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task AddBalanceAccountNotFoundTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(null as Account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        await Assert.ThrowsAsync<AccountNotFoundException>(() => accountsService.AddBalanceAsync(account.Id, 10));
        
        // Assert
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
        accountsRepositoryMock.Verify(r => r.UpdateAsync(account), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task SubtractBalanceSuccessTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        var initialBalance = account.Balance;
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        await accountsService.SubtractBalanceAsync(account.Id, 10);
        
        // Assert
        Assert.Equal(initialBalance - 10, account.Balance);
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
        accountsRepositoryMock.Verify(r => r.UpdateAsync(account), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task SubtractBalanceAccountNotFoundTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(null as Account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        await Assert.ThrowsAsync<AccountNotFoundException>(() => accountsService.SubtractBalanceAsync(account.Id, 10));
        
        // Assert
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
        accountsRepositoryMock.Verify(r => r.UpdateAsync(account), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
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