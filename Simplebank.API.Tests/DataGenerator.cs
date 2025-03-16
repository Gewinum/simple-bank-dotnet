using Simplebank.Domain.Database.Models;

namespace Simplebank.API.Tests;

public class DataGenerator
{
    public static string RandomString(string prefix) => $"prefix-{Guid.NewGuid()}";
    
    public static decimal RandomDecimal(int minimal, int maximal) => Convert.ToDecimal(new Random().NextDouble() * (maximal - minimal) + minimal);
    
    public static string RandomEmail() => $"{Guid.NewGuid()}@example.com";
    
    public static Account RandomAccount(Guid ownerId, string currency) => new()
    {
        Id = Guid.NewGuid(),
        OwnerId = ownerId,
        Currency = currency,
        Balance = RandomDecimal(0, 1000)
    };
    
    public static User RandomUser() => new()
    {
        Id = Guid.NewGuid(),
        Login = RandomString("login"),
        Name = RandomString("Name"),
        Password = RandomString("password"),
        Email = RandomEmail()
    };
}