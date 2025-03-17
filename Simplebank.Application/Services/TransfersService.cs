using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Database;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;
using Simplebank.Domain.Models.Transfers;

namespace Simplebank.Application.Services;

public class TransfersService : ITransfersService
{
    private readonly ITransfersRepository _transfersRepository;
    private readonly IEntriesRepository _entriesRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public TransfersService(ITransfersRepository transfersRepository, IEntriesRepository entriesRepository, IAccountsRepository accountsRepository, IUnitOfWork unitOfWork)
    {
        _transfersRepository = transfersRepository;
        _entriesRepository = entriesRepository;
        _accountsRepository = accountsRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<TransferResult> TransferAsync(Guid userId, Guid fromAccountId, Guid toAccountId, decimal amount)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await TransferInternalAsync(userId, fromAccountId, toAccountId, amount);
            await _unitOfWork.CommitTransactionAsync();
            return result;
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw e.InnerException ?? e;
        }
    }

    private async Task<TransferResult> TransferInternalAsync(Guid userId, Guid fromAccountId, Guid toAccountId, decimal amount)
    {
        var reverseOrder = fromAccountId.CompareTo(toAccountId) > 0;
        
        Account fromAccount;
        Account toAccount;
        
        if (reverseOrder)
        {
            toAccount = await _accountsRepository.GetWithLockAsync(toAccountId) ?? throw new AccountNotFoundException(toAccountId);
            fromAccount = await _accountsRepository.GetWithLockAsync(fromAccountId) ?? throw new AccountNotFoundException(fromAccountId);
        }
        else
        {
            fromAccount = await _accountsRepository.GetWithLockAsync(fromAccountId) ?? throw new AccountNotFoundException(fromAccountId);
            toAccount = await _accountsRepository.GetWithLockAsync(toAccountId) ?? throw new AccountNotFoundException(toAccountId);
        }
        
        if (fromAccount.OwnerId != userId)
        {
            throw new AccountNotOwnedException(userId, fromAccountId);
        }
        
        if (fromAccount.Balance < amount)
        {
            throw new InsufficientBalanceException(fromAccountId, amount);
        }
        
        fromAccount.Balance -= amount;
        toAccount.Balance += amount;
        
        await _accountsRepository.UpdateAsync(fromAccount);
        await _accountsRepository.UpdateAsync(toAccount);
        
        var transaction = await _transfersRepository.AddAsync(new Transfer
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount
        }) ?? throw new Exception("Failed to create transfer");
        
        var fromEntry = await _entriesRepository.AddAsync(new Entry
        {
            AccountId = fromAccountId,
            Amount = -amount,
            Description = "Transfer ID " + transaction.Id
        }) ?? throw new Exception("Failed to create from entry");
        
        var toEntry = await _entriesRepository.AddAsync(new Entry
        {
            AccountId = toAccountId,
            Amount = amount,
            Description = "Transfer ID " + transaction.Id
        }) ?? throw new Exception("Failed to create to entry");
        
        return new TransferResult
        {
            FromAccount = fromAccount,
            ToAccount = toAccount,
            Amount = amount,
            FromEntry = fromEntry,
            ToEntry = toEntry
        };
    }
}