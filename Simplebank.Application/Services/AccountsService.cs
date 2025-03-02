using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;

namespace Simplebank.Application.Services;

public class AccountsService : IAccountsService
{
    private readonly IAccountsRepository _accountsRepository;
    
    public AccountsService(IAccountsRepository accountsRepository, ILogger<AccountsService> logger)
    {
        _accountsRepository = accountsRepository;
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
        var transaction = await _accountsRepository.BeginTransactionAsync();
        try
        {
            var entityEntry = await _accountsRepository.AddAsync(new Account
            {
                Owner = owner,
                Balance = 0,
                Currency = currency
            });
            await _accountsRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            return entityEntry?.Entity;
        }
        catch (DbUpdateException e)
        {
            await transaction.RollbackAsync();
            if (e.InnerException is SqlException innerException)
            {
                if (innerException.Number == 2601)
                {
                    throw new AccountAlreadyExistsException(owner, currency);
                }
            }

            throw e.InnerException ?? e;
        }
    }
    
    public async Task AddBalanceAsync(Guid id, decimal amount)
    {
        var transaction = await _accountsRepository.BeginTransactionAsync();
        try
        {
            var account = await _accountsRepository.GetByIdAsync(id);
            if (account == null)
            {
                throw new AccountNotFoundException(id);
            }

            account.Balance += amount;
            _accountsRepository.Update(account);
            await _accountsRepository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException e)
        {
            await transaction.RollbackAsync();
            throw e.InnerException ?? e;
        }
    }
    
    public async Task SubtractBalanceAsync(Guid id, decimal amount)
    {
        var transaction = await _accountsRepository.BeginTransactionAsync();
        try
        {
            var account = await _accountsRepository.GetByIdAsync(id);
            if (account == null)
            {
                throw new AccountNotFoundException(id);
            }
            
            if (account.Balance < amount)
            {
                throw new InsufficientBalanceException(id, amount);
            }

            account.Balance -= amount;
            _accountsRepository.Update(account);
            await _accountsRepository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException e)
        {
            await transaction.RollbackAsync();
            throw e.InnerException ?? e;
        }
    }
}