using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Simplebank.API.Requests.Accounts;
using Simplebank.API.Requests.Users;
using Simplebank.Domain.Constants;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Models.Users;

namespace Simplebank.API.Tests;

public class AccountTests 
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AccountTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAndCreateAccount()
    {
        // Arrange
        var client = _factory.CreateClient();

        var (_, token) = await client.CreateRandomUserAndTokenAsync();
        
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        // Act
        var account = await client.CreateAccountAsync(CurrencyConstants.Currencies[0]);
        
        // Get account
        var getResponse = await client.GetAsync($"/accounts/{account.Id}");
        
        // Assert
        getResponse.EnsureSuccessStatusCode();
        var accountResponse = await getResponse.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(accountResponse);
        Assert.Equal(account.Id, accountResponse.Id);
        Assert.Equal(account.OwnerId, accountResponse.OwnerId);
        Assert.Equal(account.Currency, accountResponse.Currency);
        Assert.Equal(account.Balance, accountResponse.Balance);
    }

    [Fact]
    public async Task CreateAccountDuplicate()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var (_, token) = await client.CreateRandomUserAndTokenAsync();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        // Act
        var createAccountRequest = new CreateAccountRequest
        {
            Currency = "USD",
        };

        await client.CreateAccountAsync("USD");
        
        var createResponse2 = await client.PostAsJsonAsync("/accounts", createAccountRequest);
        Assert.Equal(HttpStatusCode.Conflict, createResponse2.StatusCode);
    }

    [Fact]
    public async Task GetUnexistingAccount()
    {
        var client = _factory.CreateClient();
        
        var (_, token) = await client.CreateRandomUserAndTokenAsync();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        var response = await client.GetAsync($"/accounts/{Guid.NewGuid()}");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddAndReduceBalance()
    {
        var client = _factory.CreateClient();
        
        var (_, token) = await client.CreateRandomUserAndTokenAsync();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var account = await client.CreateAccountAsync("USD");
        
        var addBalanceRequest = new ChangeBalanceRequest
        {
            AccountId = account.Id,
            Amount = 100
        };
        
        var addBalanceResponse = await client.PostAsJsonAsync("/accounts/balance", addBalanceRequest);
        
        addBalanceResponse.EnsureSuccessStatusCode();
        
        var accountResponse = await client.GetAsync($"/accounts/{account.Id}");
        
        accountResponse.EnsureSuccessStatusCode();
        
        var accountAfterAdd = await accountResponse.Content.ReadFromJsonAsync<Account>();
        
        Assert.NotNull(accountAfterAdd);
        Assert.Equal(account.Balance + 100, accountAfterAdd.Balance);
        
        var reduceBalanceRequest = new ChangeBalanceRequest
        {
            AccountId = account.Id,
            Amount = -50
        };
        
        var reduceBalanceResponse = await client.PostAsJsonAsync("/accounts/balance", reduceBalanceRequest);
        
        reduceBalanceResponse.EnsureSuccessStatusCode();
        
        var accountResponseAfterReduce = await client.GetAsync($"/accounts/{account.Id}");
        
        accountResponseAfterReduce.EnsureSuccessStatusCode();
        
        var accountAfterReduce = await accountResponseAfterReduce.Content.ReadFromJsonAsync<Account>();
        
        Assert.NotNull(accountAfterReduce);
        Assert.Equal(accountAfterAdd.Balance - 50, accountAfterReduce.Balance);
    }
}