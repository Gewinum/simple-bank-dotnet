using Moq;
using Simplebank.Application.Services;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Database;
using Simplebank.Domain.Interfaces.Repositories;

namespace Simplebank.Application.Tests.Services;

public class TransfersServiceTests
{
    [Fact]
    public async Task TransferSuccessTest()
    {
        // Arrange
        var sourceAccount = DataGenerator.RandomAccount(Guid.NewGuid(), "USD");
        var targetAccount = DataGenerator.RandomAccount(Guid.NewGuid(), "USD");
        var transfersRepositoryMock = new Mock<ITransfersRepository>();
        var accountsRepositoryMock = new Mock<IAccountsRepository>();
        var entriesRepositoryMock = new Mock<IEntriesRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        accountsRepositoryMock.Setup(r => r.GetWithLockAsync(sourceAccount.Id)).ReturnsAsync(sourceAccount);
        accountsRepositoryMock.Setup(r => r.GetWithLockAsync(targetAccount.Id)).ReturnsAsync(targetAccount);
        entriesRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Entry>())).ReturnsAsync(DataGenerator.RandomEntry(sourceAccount.Id));
        entriesRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Entry>())).ReturnsAsync(DataGenerator.RandomEntry(targetAccount.Id));
        transfersRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Transfer>())).ReturnsAsync(new Transfer
        {
            Id = Guid.NewGuid(),
            FromAccountId = sourceAccount.Id,
            ToAccountId = targetAccount.Id,
            Amount = 100
        });
        
        var transfersService = new TransfersService(transfersRepositoryMock.Object, entriesRepositoryMock.Object, accountsRepositoryMock.Object, unitOfWorkMock.Object);
        
        // Act
        await transfersService.TransferAsync(sourceAccount.OwnerId, sourceAccount.Id, targetAccount.Id, 100);
        
        // Assert
        accountsRepositoryMock.Verify(r => r.GetWithLockAsync(sourceAccount.Id), Times.Once);
        accountsRepositoryMock.Verify(r => r.GetWithLockAsync(targetAccount.Id), Times.Once);
        accountsRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Exactly(2));
        entriesRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Entry>()), Times.Exactly(2));
        transfersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Transfer>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }
}