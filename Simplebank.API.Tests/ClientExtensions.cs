using System.Net.Http.Json;
using Simplebank.API.Requests.Accounts;
using Simplebank.API.Requests.Users;
using Simplebank.Domain.Constants;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Models.Users;

namespace Simplebank.API.Tests;

public static class ClientExtensions
{
    public static async Task<User> CreateUserAsync(this HttpClient client)
    {
        var createUserRequest = new CreateUserRequest
        {
            Login = DataGenerator.RandomString("login"),
            Name = DataGenerator.RandomString("name"),
            Email = DataGenerator.RandomEmail(),
            Password = DataGenerator.RandomString("password")
        };
        
        var response = await client.PostAsJsonAsync("/users", createUserRequest);
        response.EnsureSuccessStatusCode();
        var userResponse = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(userResponse);
        return new User
        {
            Id = userResponse.Id,
            Login = userResponse.Login,
            Name = userResponse.Name,
            Email = userResponse.Email,
            Password = createUserRequest.Password
        };
    }

    public static async Task<Account> CreateAccountAsync(this HttpClient client, string currency)
    {
        var createAccountRequest = new CreateAccountRequest
        {
            Currency = currency
        };
        
        var response = await client.PostAsJsonAsync("/accounts", createAccountRequest);
        response.EnsureSuccessStatusCode();
        var account = await response.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        return account;
    }
    
    public static async Task<string> AuthenticateUserAsync(this HttpClient client, string login, string password)
    {
        var loginRequest = new LoginRequest
        {
            Login = login,
            Password = password
        };
        
        var response = await client.PostAsJsonAsync("/users/login", loginRequest);
        response.EnsureSuccessStatusCode();
        var tokenResponse = await response.Content.ReadFromJsonAsync<LoginResult>();
        Assert.NotNull(tokenResponse);
        return tokenResponse.Token;
    }
    
    public static async Task<(User, string)> CreateRandomUserAndTokenAsync(this HttpClient client)
    {
        var user = await client.CreateUserAsync();
        var token = await client.AuthenticateUserAsync(user.Login, user.Password);
        return (user, token);
    }
}