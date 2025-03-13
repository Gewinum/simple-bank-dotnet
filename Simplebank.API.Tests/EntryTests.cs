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
        
        var (user, token) = CreateRandomUserAndToken(client);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var accountCreateRequest = new CreateAccountRequest
        {
            Currency = CurrencyConstants.Currencies[0], 
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
    
    private (User, string) CreateRandomUserAndToken(HttpClient client)
    {
        var creationParam = new CreateUserRequest
        {
            Login = RandomString("login"),
            Name = RandomString("name"),
            Password = RandomString("password"),
            Email = RandomEmail()
        };
        
        var userResponse = client.PostAsJsonAsync("/users", creationParam).Result;
        userResponse.EnsureSuccessStatusCode();
        var userDto = userResponse.Content.ReadFromJsonAsync<UserDto>().Result;
        Assert.NotNull(userDto);
        
        var loginResponse = client.PostAsJsonAsync("/users/login", new LoginRequest
        {
            Login = creationParam.Login,
            Password = creationParam.Password
        }).Result;
        
        loginResponse.EnsureSuccessStatusCode();
        var authResult = loginResponse.Content.ReadFromJsonAsync<AuthenticationResult>().Result;
        Assert.NotNull(authResult);
        
        return (new User
        {
            Id = userDto.Id,
            Login = creationParam.Login,
            Name = creationParam.Name,
            Email = creationParam.Email,
            Password = creationParam.Password
        }, authResult.Token);
    }
    
    private string RandomString(string prefix) => $"{prefix}_{Guid.NewGuid()}";
    
    private string RandomEmail() => $"{Guid.NewGuid()}@example.com";
}