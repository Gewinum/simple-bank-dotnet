using Simplebank.Domain.Models.Tokens;

namespace Simplebank.Domain.Interfaces.Providers;

public interface ITokensProvider
{
    string GenerateToken(Guid userId);
    
    TokenInfo? ValidateToken(string token);
}