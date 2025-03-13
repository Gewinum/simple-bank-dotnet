using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Users;

public class IncorrectPasswordException : Exception, IIdentifiableException
{
    public IncorrectPasswordException(string login)
    {
        Login = login;
    }
    
    public string Login { get; }
    
    public string ErrorType => "IncorrectPassword";
    
    public override string Message => $"Incorrect password for user with login {Login}";
}