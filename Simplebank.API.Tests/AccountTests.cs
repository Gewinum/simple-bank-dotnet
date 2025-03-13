using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Simplebank.API.Requests.Accounts;
using Simplebank.API.Requests.Users;
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
        
        var (user, token) = CreateRandomUserAndToken(client);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        // Act
        var createAccountRequest = new CreateAccountRequest
        {
            Currency = "USD",
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
        Assert.Equal(account.OwnerId, accountResponse.OwnerId);
        Assert.Equal(account.Currency, accountResponse.Currency);
        Assert.Equal(account.Balance, accountResponse.Balance);
    }

    [Fact]
    public async Task CreateAccountDuplicate()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var (user, token) = CreateRandomUserAndToken(client);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        // Act
        var createAccountRequest = new CreateAccountRequest
        {
            Currency = "USD",
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
        
        var (user, token) = CreateRandomUserAndToken(client);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        var response = await client.GetAsync($"/accounts/{Guid.NewGuid()}");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddAndReduceBalance()
    {
        var client = _factory.CreateClient();
        
        var (user, token) = CreateRandomUserAndToken(client);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        var createAccountRequest = new CreateAccountRequest
        {
            Currency = "USD",
        };
        
        var createResponse = await client.PostAsJsonAsync("/accounts", createAccountRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var account = await createResponse.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        
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