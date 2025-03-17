using Paseto;
using Paseto.Builder;
using Paseto.Cryptography.Key;
using Paseto.Protocol;
using Simplebank.Domain.Interfaces.Providers;
using Simplebank.Domain.Models.Tokens;

namespace Simplebank.Application.Providers;

public class TokensProvider : ITokensProvider
{
    private readonly PasetoSymmetricKey _pasetoSymmetricKey;

    public TokensProvider()
    {
        _pasetoSymmetricKey = new PasetoBuilder().UseV4(Purpose.Local)
            .GenerateSymmetricKey();
    }

    public TokensProvider(byte[] key)
    {
        _pasetoSymmetricKey = new PasetoSymmetricKey(key, new Version4());
    }
        
    public string GenerateToken(Guid userId)
    {
        return new PasetoBuilder()
            .UseV4(Purpose.Local)
            .WithKey(_pasetoSymmetricKey)
            .Subject(userId.ToString())
            .Expiration(DateTime.UtcNow.AddHours(1))
            .Encode();
    }
        
    public TokenInfo? ValidateToken(string token)
    {
        try
        {
            var valParams = new PasetoTokenValidationParameters
            {
                ValidateLifetime = true
            };

            var payload = new PasetoBuilder()
                .UseV4(Purpose.Local)
                .WithKey(_pasetoSymmetricKey)
                .Decode(token, valParams);

            if (payload == null || !payload.IsValid)
            {
                return null;
            }

            var subject = payload.Paseto.Payload["sub"]?.ToString();
            if (string.IsNullOrEmpty(subject))
            {
                return null;
            }

            return new TokenInfo
            {
                UserId = Guid.Parse(subject)
            };
        }
        catch
        {
            return null;
        }
    }
}