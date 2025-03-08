using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Simplebank.API.Requests.Accounts;
using Simplebank.API.Requests.Transfers;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Models.Transfers;

namespace Simplebank.API.Tests;

public class TransferTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TransferTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TestTransferSuccess()
    {
        var client = _factory.CreateClient();
        
        var accountFrom = await CreateAccount(client, RandomString("Owner"), "USD");
        var accountTo = await CreateAccount(client, RandomString("Owner"), "USD");

        await AddBalance(client, accountFrom.Id, 1000);
        await AddBalance(client, accountTo.Id, 1000);
        
        accountFrom.Balance += 1000;
        accountTo.Balance += 1000;

        var transferResult = await ExecuteTransfer(client, accountFrom.Id, accountTo.Id, 100);
        
        Assert.NotNull(transferResult);
        Assert.Equal(100, transferResult.Amount);
        Assert.Equal(accountFrom.Id, transferResult.FromAccount.Id);
        Assert.Equal(accountFrom.Balance - 100, transferResult.FromAccount.Balance);
        Assert.Equal(accountTo.Id, transferResult.ToAccount.Id);
        Assert.Equal(accountTo.Balance + 100, transferResult.ToAccount.Balance);
        Assert.Equal(accountFrom.Id, transferResult.FromEntry.AccountId);
        Assert.Equal(-100, transferResult.FromEntry.Amount);
        Assert.Equal(accountTo.Id, transferResult.ToEntry.AccountId);
        Assert.Equal(100, transferResult.ToEntry.Amount);
    }
    
    [Fact]
    public async Task TestTransferSourceAccountNotFound()
    {
        var client = _factory.CreateClient();
        
        var accountTo = await CreateAccount(client, RandomString("Owner"), "USD");
        
        await AddBalance(client, accountTo.Id, 1000);
        
        var transferRequest = new TransferRequest
        {
            FromAccount = Guid.NewGuid(),
            ToAccount = accountTo.Id,
            Amount = 100
        };
        
        var transferResponse = await client.PostAsJsonAsync("/transfers", transferRequest);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, transferResponse.StatusCode);
    }
    
    [Fact]
    public async Task TestTransferDestinationAccountNotFound()
    {
        var client = _factory.CreateClient();
        
        var accountFrom = await CreateAccount(client, RandomString("Owner"), "USD");
        
        await AddBalance(client, accountFrom.Id, 1000);
        
        var transferRequest = new TransferRequest
        {
            FromAccount = accountFrom.Id,
            ToAccount = Guid.NewGuid(),
            Amount = 100
        };
        
        var transferResponse = await client.PostAsJsonAsync("/transfers", transferRequest);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, transferResponse.StatusCode);
    }
    
    [Fact]
    public async Task TestTransferInsufficientBalance()
    {
        var client = _factory.CreateClient();
        
        var accountFrom = await CreateAccount(client, RandomString("Owner"), "USD");
        var accountTo = await CreateAccount(client, RandomString("Owner"), "USD");
        
        await AddBalance(client, accountFrom.Id, 1000);
        await AddBalance(client, accountTo.Id, 1000);
        
        var transferRequest = new TransferRequest
        {
            FromAccount = accountFrom.Id,
            ToAccount = accountTo.Id,
            Amount = 10000
        };
        
        var transferResponse = await client.PostAsJsonAsync("/transfers", transferRequest);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, transferResponse.StatusCode);
    }
    
    [Fact]
    public async Task TestParallelTransfers()
    {
        var client = _factory.CreateClient();

        var accountFrom = await CreateAccount(client, RandomString("Owner"), "USD");
        var accountTo = await CreateAccount(client, RandomString("Owner"), "USD");

        await AddBalance(client, accountFrom.Id, 10000);
        await AddBalance(client, accountTo.Id, 10000);

        var transferTasks = new List<Task>();

        for (int i = 0; i < 26; i++)
        {
            if (i % 2 == 0)
            {
                transferTasks.Add(ExecuteTransferAndCheck(client, accountFrom.Id, accountTo.Id, 100));
            }
            else
            {
                transferTasks.Add(ExecuteTransferAndCheck(client, accountTo.Id, accountFrom.Id, 100));
            }
        }

        await Task.WhenAll(transferTasks);

        var updatedFromAccount = await GetAccount(client, accountFrom.Id);
        var updatedToAccount = await GetAccount(client, accountTo.Id);

        Assert.Equal(10000, updatedFromAccount.Balance);
        Assert.Equal(10000, updatedToAccount.Balance);
    }
    
    private async Task<TransferResult> ExecuteTransferAndCheck(HttpClient client, Guid fromAccount, Guid toAccount, decimal amount)
    {
        var transferResult = await ExecuteTransfer(client, fromAccount, toAccount, amount);

        var fromAccountBalance = transferResult.FromAccount.Balance;
        var toAccountBalance = transferResult.ToAccount.Balance;
        
        if (Math.Abs(fromAccountBalance - toAccountBalance) % (amount * 2) != 0)
        {
            throw new Exception($"Incorrect balance after transfer: {fromAccountBalance} {toAccountBalance}");
        }
        
        return transferResult;
    }
    
    private async Task<TransferResult> ExecuteTransfer(HttpClient client, Guid fromAccount, Guid toAccount, decimal amount)
    {
        var transferRequest = new TransferRequest
        {
            FromAccount = fromAccount,
            ToAccount = toAccount,
            Amount = amount
        };
        
        var transferResponse = await client.PostAsJsonAsync("/transfers", transferRequest);
        transferResponse.EnsureSuccessStatusCode();
        
        var transferResult = await transferResponse.Content.ReadFromJsonAsync<TransferResult>();
        Assert.NotNull(transferResult);
        return transferResult;
    }
    
    private async Task<Account> GetAccount(HttpClient client, Guid accountId)
    {
        var response = await client.GetAsync($"/accounts/{accountId}");
        response.EnsureSuccessStatusCode();
        var account = await response.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        return account;
    }
    
    private async Task<Account> CreateAccount(HttpClient client, string owner, string currency)
    {
        var accountCreateRequest = new CreateAccountRequest
        {
            Currency = currency,
            Owner = owner
        };
        
        var createAccountResponse = await client.PostAsJsonAsync("/accounts", accountCreateRequest);
        createAccountResponse.EnsureSuccessStatusCode();
        
        var account = await createAccountResponse.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        return account;
    }
    
    private async Task AddBalance(HttpClient client, Guid accountId, decimal amount)
    {
        var addBalanceRequest = new ChangeBalanceRequest
        {
            AccountId = accountId,
            Amount = amount
        };
        
        var addBalanceResponse = await client.PostAsJsonAsync("/accounts/balance", addBalanceRequest);
        addBalanceResponse.EnsureSuccessStatusCode();
    }
    
    private string RandomString(string prefix) => $"{prefix}_{Guid.NewGuid()}";
}