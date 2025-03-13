using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Users;

public class UserAlreadyExistsException : Exception, IIdentifiableException
{
    public UserAlreadyExistsException(string login, string email)
    {
        Login = login;
        Email = email;
    }
    
    public string Login { get; }
    
    public string Email { get; }
    
    public string ErrorType => "UserAlreadyExists";
    
    public override string Message => $"User with login {Login} or email {Email} already exists";
}