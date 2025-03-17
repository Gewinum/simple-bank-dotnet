using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Simplebank.API.Requests.Users;
using Simplebank.Domain.Models.Users;

namespace Simplebank.API.Tests;

public class UserTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUserWithoutAuthTest()
    {
        var client = _factory.CreateClient();

        var user = await client.CreateUserAsync();
        
        var getUserRequest = await client.GetAsync($"/users/{user.Id}");
        Assert.Equal(HttpStatusCode.Unauthorized, getUserRequest.StatusCode);
    }

    [Fact]
    public async Task GetUserWithIncorrectToken()
    {
        var client = _factory.CreateClient();

        var user = await client.CreateUserAsync();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer invalidToken");
        
        var getUserRequest = await client.GetAsync($"/users/{user.Id}");
        Assert.Equal(HttpStatusCode.Unauthorized, getUserRequest.StatusCode);
    }
    
    [Fact]
    public async Task CreateUserAndLogin()
    {
        var client = _factory.CreateClient();
        
        var user = await client.CreateUserAsync();
        var token = await client.AuthenticateUserAsync(user.Login, user.Password);
        Assert.NotNull(token);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        var getUserRequest = await client.GetAsync($"/users/{user.Id}");
        getUserRequest.EnsureSuccessStatusCode();
        var userResponse = await getUserRequest.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(userResponse);
        Assert.Equal(user.Id, userResponse.Id);
        Assert.Equal(user.Login, userResponse.Login);
        Assert.Equal(user.Name, userResponse.Name);
        Assert.Equal(user.Email, userResponse.Email);
    }
    
    
    [Fact]
    public async Task CreateUserDuplicate()
    {
        var client = _factory.CreateClient();

        var user = await client.CreateUserAsync();
        var createUserRequest = new CreateUserRequest
        {
            Login = user.Login,
            Name = user.Name,
            Email = user.Email,
            Password = user.Password
        };

        var response = await client.PostAsJsonAsync("/users", createUserRequest);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task InvalidLogin()
    {
        var client = _factory.CreateClient();

        var loginRequest = new LoginRequest
        {
            Login = "invalidLogin",
            Password = "invalidPassword"
        };

        var response = await client.PostAsJsonAsync("/users/login", loginRequest);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task InvalidPassword()
    {
        var client = _factory.CreateClient();

        var user = await client.CreateUserAsync();
        var loginRequest = new LoginRequest
        {
            Login = user.Login,
            Password = "invalidPassword"
        };

        var response = await client.PostAsJsonAsync("/users/login", loginRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}