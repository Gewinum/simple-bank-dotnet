﻿using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Simplebank.API.Requests.Accounts;
using Simplebank.Domain.Database.Models;

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
        
        // Act
        var createAccountRequest = new CreateAccountRequest
        {
            Currency = "USD",
            Owner = RandomString("Owner")
        };
        var createResponse = await client.PostAsJsonAsync("/accounts", createAccountRequest);
        createResponse.EnsureSuccessStatusCode();
        var account = await createResponse.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        
        // Get account
        var getResponse = await client.GetAsync($"/accounts/{account.Id}");
        
        // Assert
        getResponse.EnsureSuccessStatusCode();
        var accountResponse = await getResponse.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(accountResponse);
        Assert.Equal(account.Id, accountResponse.Id);
        Assert.Equal(account.Owner, accountResponse.Owner);
        Assert.Equal(account.Currency, accountResponse.Currency);
        Assert.Equal(account.Balance, accountResponse.Balance);
    }

    [Fact]
    public async Task CreateAccountDuplicate()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var createAccountRequest = new CreateAccountRequest
        {
            Currency = "USD",
            Owner = RandomString("Owner")
        };
        
        var createResponse = await client.PostAsJsonAsync("/accounts", createAccountRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var createResponse2 = await client.PostAsJsonAsync("/accounts", createAccountRequest);
        Assert.Equal(HttpStatusCode.Conflict, createResponse2.StatusCode);
    }

    [Fact]
    public async Task GetUnexistingAccount()
    {
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync($"/accounts/{Guid.NewGuid()}");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddAndReduceBalance()
    {
        var client = _factory.CreateClient();
        
        var createAccountRequest = new CreateAccountRequest
        {
            Currency = "USD",
            Owner = RandomString("Owner")
        };
        
        var createResponse = await client.PostAsJsonAsync("/accounts", createAccountRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var account = await createResponse.Content.ReadFromJsonAsync<Account>();
        
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
    
    private string RandomString(string prefix) => $"{prefix}_{Guid.NewGuid()}";
}