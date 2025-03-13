using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Database;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;
using Simplebank.Infrastructure.Exceptions;

namespace Simplebank.Application.Services;

public class AccountsService : IAccountsService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IEntriesRepository _entriesRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public AccountsService(IAccountsRepository accountsRepository, IEntriesRepository entriesRepository, IUnitOfWork unitOfWork)
    {
        _accountsRepository = accountsRepository;
        _entriesRepository = entriesRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Account> GetAccountAsync(Guid userId, Guid id)
    {
        var account = await _accountsRepository.GetByIdAsync(id);
        if (account == null)
        {
            throw new AccountNotFoundException(id);
        }
        if (account.OwnerId != userId)
        {
            throw new AccountNotOwnedException(userId, id);
        }
        return account;
    }
    
    public async Task<IEnumerable<Account>> GetAccountsAsync(Guid userId)
    {
        return await _accountsRepository.GetByOwnerAsync(userId);
    }
    
    public async Task<Account?> CreateAccountAsync(Guid ownerId, string currency)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var account = await _accountsRepository.AddAsync(new Account
            {
                OwnerId = ownerId,
                Balance = 0,
                Currency = currency
            });
            await _unitOfWork.CommitTransactionAsync();
            return account;
        }
        catch (DuplicateKeysException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new AccountAlreadyExistsException(ownerId, currency);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw e.InnerException ?? e;
        }
    }
    
    public async Task<Entry> AddBalanceAsync(Guid userId, Guid id, decimal amount)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var account = await _accountsRepository.AddBalanceAsync(id, amount);
            if (account == null)
            {
                throw new AccountNotFoundException(id);
            }
            
            if (account.OwnerId != userId)
            {
                throw new AccountNotOwnedException(userId, id);
            }

            if (account.Balance <= decimal.Zero)
            {
                throw new InsufficientBalanceException(account.Id, amount);
            }

            var entry = await _entriesRepository.AddAsync(new Entry
            {
                AccountId = id,
                Amount = amount,
                Description = "Balance added by an API"
            });
            if (entry == null)
            {
                throw new Exception("Unexpectedly failed to add entry");
            }
            await _unitOfWork.CommitTransactionAsync();
            return entry;
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw e.InnerException ?? e;
        }
    }
}