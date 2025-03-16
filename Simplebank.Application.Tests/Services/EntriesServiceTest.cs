using Moq;
using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Application.Services;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Repositories;

namespace Simplebank.Application.Tests.Services;

public class EntriesServiceTest
{
    [Fact]
    public async Task GetEntriesAsyncSuccess()
    {
        var account = DataGenerator.RandomAccount(Guid.NewGuid(), "USD");
        var entries = DataGenerator.RandomEntries(account.Id, 5);
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        entriesRepositoryMock.Setup(r => r.GetEntriesAsync(account.Id, 1, 5)).ReturnsAsync(entries);
        var entriesService = new EntriesService(entriesRepositoryMock.Object, accountsRepositoryMock.Object);
        
        var result = await entriesService.GetEntriesAsync(account.OwnerId, account.Id, 1, 5);
        
        Assert.Equal(entries, result);
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
        entriesRepositoryMock.Verify(r => r.GetEntriesAsync(account.Id, 1, 5), Times.Once);
    }
    
    [Fact]
    public async Task GetEntriesAsyncAccountNotFound()
    {
        var account = DataGenerator.RandomAccount(Guid.NewGuid(), "USD");
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(null as Account);
        var entriesService = new EntriesService(entriesRepositoryMock.Object, accountsRepositoryMock.Object);

        await Assert.ThrowsAsync<AccountNotFoundException>(() => entriesService.GetEntriesAsync(account.OwnerId, account.Id, 1, 5));
        
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
        entriesRepositoryMock.Verify(r => r.GetEntriesAsync(account.Id, 1, 5), Times.Never);
    }
    
    [Fact]
    public async Task GetEntriesAsyncAccountNotOwned()
    {
        var account = DataGenerator.RandomAccount(Guid.NewGuid(), "USD");
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        var entriesService = new EntriesService(entriesRepositoryMock.Object, accountsRepositoryMock.Object);

        await Assert.ThrowsAsync<AccountNotOwnedException>(() => entriesService.GetEntriesAsync(Guid.NewGuid(), account.Id, 1, 5));
        
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
        entriesRepositoryMock.Verify(r => r.GetEntriesAsync(account.Id, 1, 5), Times.Never);
    }
    
    [Fact]
    public async Task GetEntriesAsyncNoEntries()
    {
        var account = DataGenerator.RandomAccount(Guid.NewGuid(), "USD");
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        accountsRepositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        entriesRepositoryMock.Setup(r => r.GetEntriesAsync(account.Id, 1, 5)).ReturnsAsync(new List<Entry>());
        var entriesService = new EntriesService(entriesRepositoryMock.Object, accountsRepositoryMock.Object);
        
        var result = await entriesService.GetEntriesAsync(account.OwnerId, account.Id, 1, 5);
        
        Assert.Empty(result);
        accountsRepositoryMock.Verify(r => r.GetByIdAsync(account.Id), Times.Once);
        entriesRepositoryMock.Verify(r => r.GetEntriesAsync(account.Id, 1, 5), Times.Once);
    }
}