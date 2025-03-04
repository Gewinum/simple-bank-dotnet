using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Simplebank.API.Requests.Accounts;
using Simplebank.Domain.Constants;
using Simplebank.Domain.Database.Models;

namespace Simplebank.API.Tests;

public class EntryTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EntryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task EntriesAreBeingCreated()
    {
        var client = _factory.CreateClient();

        var accountCreateRequest = new CreateAccountRequest
        {
            Currency = CurrencyConstants.Currencies[0],
            Owner = RandomString("Owner")
        };
        
        var createAccountResponse = await client.PostAsJsonAsync("/accounts", accountCreateRequest);
        createAccountResponse.EnsureSuccessStatusCode();
        
        var account = await createAccountResponse.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        
        var addBalanceRequest = new ChangeBalanceRequest
        {
            AccountId = account.Id,
            Amount = 100
        };
        
        var addBalanceResponse = await client.PostAsJsonAsync("/accounts/balance", addBalanceRequest);
        addBalanceResponse.EnsureSuccessStatusCode();
        
        var entries = await client.GetFromJsonAsync<Entry[]>($"/entries/{account.Id}?page=1&perPage=10");
        Assert.NotNull(entries);
        Assert.Single(entries);
        
        var entry = entries[0];
        Assert.Equal(account.Id, entry.AccountId);
        Assert.Equal(100, entry.Amount);
    }
    
    private string RandomString(string prefix) => $"{prefix}_{Guid.NewGuid()}";
}