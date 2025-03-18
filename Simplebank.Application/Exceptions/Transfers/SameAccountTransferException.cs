using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.Application.Exceptions.Transfers;

public class SameAccountTransferException : Exception, IIdentifiableException
{
    public SameAccountTransferException()
    {
    }
    
    public string ErrorType => "SameAccountTransfer";
    
    public override string Message => "Cannot transfer money to the same account";
}