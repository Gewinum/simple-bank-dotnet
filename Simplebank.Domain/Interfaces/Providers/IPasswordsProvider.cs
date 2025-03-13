namespace Simplebank.Domain.Interfaces.Providers;

public interface IPasswordsProvider
{
    string CreateHash(string password);

    bool VerifyPassword(string password, string hashString);
}