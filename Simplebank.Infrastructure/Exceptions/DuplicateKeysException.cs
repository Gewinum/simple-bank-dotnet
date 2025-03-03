namespace Simplebank.Infrastructure.Exceptions;

public class DuplicateKeysException : Exception
{
    public override string Message => "Can't insert row because of duplicate keys";
}