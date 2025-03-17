using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Simplebank.API.Requests.Accounts;
using Simplebank.API.Requests.Users;
using Simplebank.Domain.Constants;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Models.Users;

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
        
        var (_, token) = await client.CreateRandomUserAndTokenAsync();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var account = await client.CreateAccountAsync(CurrencyConstants.Currencies[0]);
        
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
}