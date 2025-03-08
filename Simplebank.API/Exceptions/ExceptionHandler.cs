using Simplebank.Domain.Interfaces.Exceptions;

namespace Simplebank.API.Exceptions;

public static class ExceptionHandler
{
    public static object HandleException(Exception exception)
    {
        if (exception is IIdentifiableException identifiableException)
        {
            var errorDetails = new
            {
                ErrorType = identifiableException.ErrorType,
                Message = exception.Message
            };
            return errorDetails;
        }
        else
        {
            var errorDetails = new
            {
                ErrorType = "UnknownError",
                Message = exception.Message
            };
            return errorDetails;
        }
    }
}