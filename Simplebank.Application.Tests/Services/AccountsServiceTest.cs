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
        var account = DataGenerator.RandomAccount(Guid.NewGuid(), "USD");
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, entriesRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        var result = await accountsService.GetAccountAsync(account.OwnerId, account.Id);
        
        // Assert
        Assert.Equal(account, result);
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
    }
    
    [Fact]
    public async Task GetAccountByIdNotFoundTest()
    {
        // Arrange
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        accountsRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Account);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var accountsService = new AccountsService(accountsRepositoryMock.Object, entriesRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        await Assert.ThrowsAsync<AccountNotFoundException>(() => accountsService.GetAccountAsync(Guid.NewGuid(), Guid.NewGuid()));
        
        // Assert
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetAccountNotOwnedTest()
    {
        // Arrange
        var account = DataGenerator.RandomAccount(Guid.NewGuid(), "USD");
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var accountsService = new AccountsService(accountsRepositoryMock.Object, entriesRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        await Assert.ThrowsAsync<AccountNotOwnedException>(() => accountsService.GetAccountAsync(Guid.NewGuid(), account.Id));
        
        // Assert
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
    }

    [Fact]
    public async Task CreateAccountSuccessTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        account.Balance = 0;
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Account>())).ReturnsAsync(account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, entriesRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await accountsService.CreateAccountAsync(account.OwnerId, account.Currency);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.OwnerId, result.OwnerId);
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
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Account>())).ThrowsAsync(new DuplicateKeysException());
        var accountsService = new AccountsService(accountsRepositoryMock.Object, entriesRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<AccountAlreadyExistsException>(() => accountsService.CreateAccountAsync(account.OwnerId, account.Currency));

        // Assert
        Assert.Equal(exception.OwnerId, account.OwnerId);
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
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var amount = 10;
        var testEntry = new Entry
        {
            AccountId = account.Id,
            Amount = amount,
            Description = "Testing"
        };

        accountsRepositoryMock.Setup(r => r.AddBalanceAsync(account.Id, amount)).ReturnsAsync(account);
        entriesRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Entry>())).ReturnsAsync(testEntry);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, entriesRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        await accountsService.AddBalanceAsync(account.OwnerId, account.Id, amount);
        
        // Assert
        Assert.Equal(initialBalance, account.Balance);
        accountsRepositoryMock.Verify(r => r.AddBalanceAsync(account.Id, amount), Times.Once);
        entriesRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Entry>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task AddBalanceAccountNotFoundTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.AddBalanceAsync(account.Id, 10)).ReturnsAsync(null as Account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, entriesRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        await Assert.ThrowsAsync<AccountNotFoundException>(() => accountsService.AddBalanceAsync(account.OwnerId, account.Id, 10));
        
        // Assert
        accountsRepositoryMock.Verify(r => r.AddBalanceAsync(account.Id, 10), Times.Once);
        entriesRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Entry>()), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task AddBalanceAccountInsufficientBalanceTest()
    {
        // Arrange
        var account = CreateRandomAccount();
        account.Balance -= 10000;
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        accountsRepositoryMock.Setup(r => r.AddBalanceAsync(account.Id, -10000)).ReturnsAsync(account);
        var accountsService = new AccountsService(accountsRepositoryMock.Object, entriesRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        await Assert.ThrowsAsync<InsufficientBalanceException>(() => accountsService.AddBalanceAsync(account.OwnerId, account.Id, -10000));
        
        // Assert
        accountsRepositoryMock.Verify(r => r.AddBalanceAsync(account.Id, -10000), Times.Once);
        entriesRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Entry>()), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }
    
    private Account CreateRandomAccount()
    {
        var randomCurrency = CurrencyConstants.Currencies[new Random().Next(CurrencyConstants.Currencies.Length)];
        return new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Currency = randomCurrency,
            Balance = new Random().Next(0, 1000)
        };
    }
}