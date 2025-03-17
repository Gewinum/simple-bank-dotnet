using Simplebank.Application.Providers;

namespace Simplebank.Application.Tests.Providers;

public class TokensProviderTest
{
    private readonly TokensProvider _tokensProvider;

    public TokensProviderTest()
    {
        _tokensProvider = new TokensProvider();
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var token = _tokensProvider.GenerateToken(userId);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public void ValidateToken_ShouldReturnUserId_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = _tokensProvider.GenerateToken(userId);

        // Act
        var tokenInfo = _tokensProvider.ValidateToken(token);

        // Assert
        Assert.NotNull(tokenInfo);
        Assert.Equal(userId, tokenInfo.UserId);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid-token";

        // Act
        var validatedUserId = _tokensProvider.ValidateToken(invalidToken);

        // Assert
        Assert.Null(validatedUserId);
    }
}