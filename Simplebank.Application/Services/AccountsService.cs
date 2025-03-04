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
    
    public async Task<Account> GetAccountAsync(Guid id)
    {
        var account = await _accountsRepository.GetByIdAsync(id);
        if (account == null)
        {
            throw new AccountNotFoundException(id);
        }
        return account;
    }
    
    public async Task<Account?> CreateAccountAsync(string owner, string currency)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var account = await _accountsRepository.AddAsync(new Account
            {
                Owner = owner,
                Balance = 0,
                Currency = currency
            });
            await _unitOfWork.CommitTransactionAsync();
            return account;
        }
        catch (DuplicateKeysException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new AccountAlreadyExistsException(owner, currency);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw e.InnerException ?? e;
        }
    }
    
    public async Task AddBalanceAsync(Guid id, decimal amount)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var account = await _accountsRepository.GetByIdAsync(id);
            if (account == null)
            {
                throw new AccountNotFoundException(id);
            }

            await _entriesRepository.AddAsync(new Entry
            {
                AccountId = account.Id,
                Amount = amount,
                Description = "Balance modification from API"
            });

            account.Balance += amount;
            await _accountsRepository.UpdateAsync(account);
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw e.InnerException ?? e;
        }
    }
}