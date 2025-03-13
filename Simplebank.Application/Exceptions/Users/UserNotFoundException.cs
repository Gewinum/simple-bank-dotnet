using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Users;

public class UserNotFoundException : Exception, IIdentifiableException
{
    public UserNotFoundException(Guid userId)
    {
        UserId = userId;
    }
    
    public Guid UserId { get; }
    
    public string ErrorType => "UserNotFound";
    
    public override string Message => $"User with id {UserId} not found";
}