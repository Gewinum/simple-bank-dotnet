namespace Simplebank.Domain.Interfaces.Exceptions;

public interface IIdentifiableException
{
    public string ErrorType { get; }
}