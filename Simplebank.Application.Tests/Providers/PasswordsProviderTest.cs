using Simplebank.Application.Providers;

namespace Simplebank.Application.Tests.Providers;

public class PasswordsProviderTest
{
    [Fact]
    public void CreateValidHashAndCheck()
    {
        // Arrange
        var passwordsProvider = new PasswordsProvider();
        var password = "password";

        // Act
        var hash = passwordsProvider.CreateHash(password);

        // Assert
        Assert.NotNull(hash);
        
        // Act
        var result = passwordsProvider.VerifyPassword(password, hash);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void CreateInvalidHashAndCheck()
    {
        // Arrange
        var passwordsProvider = new PasswordsProvider();
        var password = "password";
        var invalidPassword = "invalidPassword";

        // Act
        var hash = passwordsProvider.CreateHash(password);

        // Assert
        Assert.NotNull(hash);
        
        // Act
        var result = passwordsProvider.VerifyPassword(invalidPassword, hash);
        
        // Assert
        Assert.False(result);
    }
}