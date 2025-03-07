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
        
        var transferRequest = new TransferRequest
        {
            FromAccount = accountFrom.Id,
            ToAccount = accountTo.Id,
            Amount = 100
        };
        
        var transferResponse = await client.PostAsJsonAsync("/transfers", transferRequest);
        transferResponse.EnsureSuccessStatusCode();
        
        var transferResult = await transferResponse.Content.ReadFromJsonAsync<TransferResult>();
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