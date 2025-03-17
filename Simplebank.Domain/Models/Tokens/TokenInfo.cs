using System.Security.Claims;
using Simplebank.Domain.Database.Models;

namespace Simplebank.Domain.Models.Tokens;

public class TokenInfo
{
    public required Guid UserId { get; set; }
}