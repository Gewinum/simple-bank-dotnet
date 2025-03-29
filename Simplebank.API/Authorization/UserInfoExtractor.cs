using System.Security.Claims;
using Paseto;
using Simplebank.Domain.Models.Tokens;

namespace Simplebank.API.Authorization;

public static class UserInfoExtractor
{
    public static TokenInfo ExtractFromRequest(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new PasetoInvalidClaimException("User id not found in claims");
        }
        return new TokenInfo
        {
            UserId = Guid.Parse(userId)
        };
    }
}