using Simplebank.Domain.Database.Models;

namespace Simplebank.Application.Tests;

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
    
    public static Entry RandomEntry(Guid accountId) => new()
    {
        Id = Guid.NewGuid(),
        AccountId = accountId,
        Amount = RandomDecimal(-1000, 1000),
        Description = RandomString("description")
    };
    
    public static Entry[] RandomEntries(Guid accountId, int count) => Enumerable.Range(0, count).Select(_ => RandomEntry(accountId)).ToArray();
}