using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Users;

public class LoginNotFoundException : Exception, IIdentifiableException
{
    public LoginNotFoundException(string login)
    {
        Login = login;
    }
    
    public string Login { get; }
    
    public string ErrorType => "LoginNotFound";
    
    public override string Message => $"User with login {Login} not found";
}