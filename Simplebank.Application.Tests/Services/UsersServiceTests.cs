using System.Runtime.InteropServices.JavaScript;
using AutoMapper;
using Moq;
using Simplebank.Application.Exceptions.Users;
using Simplebank.Application.Services;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Database;
using Simplebank.Domain.Interfaces.Providers;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Models.Users;
using Simplebank.Infrastructure.Exceptions;

namespace Simplebank.Application.Tests.Services;

public class UsersServiceTests
{
    [Fact]
    public async Task GetUserByIdSuccessTest()
    {
        // Arrange
        var user = DataGenerator.RandomUser();
        var usersRepositoryMock = new Mock<IUsersRepository>();
        usersRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var mapperMock = new Mock<IMapper>();
        var userDto = new UserDto
        {
            Id = user.Id,
            Login = user.Login,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(userDto);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordsProviderMock = new Mock<IPasswordsProvider>();
        var tokensProviderMock = new Mock<ITokensProvider>();
        var usersService = new UsersService(usersRepositoryMock.Object, mapperMock.Object, unitOfWorkMock.Object, passwordsProviderMock.Object, tokensProviderMock.Object);
        
        // Act
        var result = await usersService.GetByIdAsync(user.Id);
        
        // Assert
        Assert.Equal(userDto, result);
        usersRepositoryMock.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
        mapperMock.Verify(m => m.Map<UserDto>(user), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdNotExistsTest()
    {
        // Arrange
        var user = DataGenerator.RandomUser();
        var usersRepositoryMock = new Mock<IUsersRepository>();
        usersRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(null as User);
        var mapperMock = new Mock<IMapper>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordsProviderMock = new Mock<IPasswordsProvider>();
        var tokensProviderMock = new Mock<ITokensProvider>();
        var usersService = new UsersService(usersRepositoryMock.Object, mapperMock.Object, unitOfWorkMock.Object, passwordsProviderMock.Object, tokensProviderMock.Object);
        
        // Act
        await Assert.ThrowsAsync<UserNotFoundException>(() => usersService.GetByIdAsync(user.Id));
        
        // Assert
        usersRepositoryMock.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
    }

    [Fact]
    public async Task CreateUserSuccessTest()
    {
        // Arrange
        var user = DataGenerator.RandomUser();
        var usersRepositoryMock = new Mock<IUsersRepository>();
        usersRepositoryMock.Setup(r => r.GetByLoginAsync(user.Login)).ReturnsAsync(null as User);
        usersRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(null as User);
        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<User>())).Returns(new UserDto
        {
            Id = user.Id,
            Login = user.Login,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordsProviderMock = new Mock<IPasswordsProvider>();
        passwordsProviderMock.Setup(p => p.CreateHash(user.Password)).Returns("hash");
        var tokensProviderMock = new Mock<ITokensProvider>();
        var usersService = new UsersService(usersRepositoryMock.Object, mapperMock.Object, unitOfWorkMock.Object, passwordsProviderMock.Object, tokensProviderMock.Object);
        
        // Act
        var result = await usersService.CreateAsync(user.Login, user.Name, user.Email, user.Password);
        
        // Assert
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Login, result.Login);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
        usersRepositoryMock.Verify(r => r.GetByLoginAsync(user.Login), Times.Once);
        usersRepositoryMock.Verify(r => r.GetByEmailAsync(user.Email), Times.Once);
        usersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateUserDuplicateLoginTest()
    {
        // Arrange
        var user = DataGenerator.RandomUser();
        var usersRepositoryMock = new Mock<IUsersRepository>();
        usersRepositoryMock.Setup(r => r.GetByLoginAsync(user.Login)).ReturnsAsync(user);
        var mapperMock = new Mock<IMapper>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordsProviderMock = new Mock<IPasswordsProvider>();
        var tokensProviderMock = new Mock<ITokensProvider>();
        var usersService = new UsersService(usersRepositoryMock.Object, mapperMock.Object, unitOfWorkMock.Object, passwordsProviderMock.Object, tokensProviderMock.Object);
        
        // Act
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => usersService.CreateAsync(user.Login, user.Name, user.Email, user.Password));
        
        // Assert
        usersRepositoryMock.Verify(r => r.GetByLoginAsync(user.Login), Times.Once);
        usersRepositoryMock.Verify(r => r.GetByEmailAsync(user.Email), Times.Never);
        usersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task CreateUserDuplicateEmailTest()
    {
        // Arrange
        var user = DataGenerator.RandomUser();
        var usersRepositoryMock = new Mock<IUsersRepository>();
        usersRepositoryMock.Setup(r => r.GetByLoginAsync(user.Login)).ReturnsAsync(null as User);
        usersRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        var mapperMock = new Mock<IMapper>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordsProviderMock = new Mock<IPasswordsProvider>();
        var tokensProviderMock = new Mock<ITokensProvider>();
        var usersService = new UsersService(usersRepositoryMock.Object, mapperMock.Object, unitOfWorkMock.Object, passwordsProviderMock.Object, tokensProviderMock.Object);
        
        // Act
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => usersService.CreateAsync(user.Login, user.Name, user.Email, user.Password));
        
        // Assert
        usersRepositoryMock.Verify(r => r.GetByLoginAsync(user.Login), Times.Once);
        usersRepositoryMock.Verify(r => r.GetByEmailAsync(user.Email), Times.Once);
        usersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateUserDuplicateKeysTest()
    {
        // Arrange
        var user = DataGenerator.RandomUser();
        var usersRepositoryMock = new Mock<IUsersRepository>();
        usersRepositoryMock.Setup(r => r.GetByLoginAsync(user.Login)).ReturnsAsync(null as User);
        usersRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(null as User);
        usersRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).ThrowsAsync(new DuplicateKeysException());
        var mapperMock = new Mock<IMapper>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordsProviderMock = new Mock<IPasswordsProvider>();
        var tokensProviderMock = new Mock<ITokensProvider>();
        var usersService = new UsersService(usersRepositoryMock.Object, mapperMock.Object, unitOfWorkMock.Object, passwordsProviderMock.Object, tokensProviderMock.Object);
        
        // Act
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => usersService.CreateAsync(user.Login, user.Name, user.Email, user.Password));
        
        // Assert
        usersRepositoryMock.Verify(r => r.GetByLoginAsync(user.Login), Times.Once);
        usersRepositoryMock.Verify(r => r.GetByEmailAsync(user.Email), Times.Once);
        usersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginUserSuccessTest()
    {
        // Arrange
        var user = DataGenerator.RandomUser();
        var usersRepositoryMock = new Mock<IUsersRepository>();
        usersRepositoryMock.Setup(r => r.GetByLoginAsync(user.Login)).ReturnsAsync(user);
        var mapperMock = new Mock<IMapper>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordsProviderMock = new Mock<IPasswordsProvider>();
        passwordsProviderMock.Setup(p => p.VerifyPassword(user.Password, user.Password)).Returns(true);
        var tokensProviderMock = new Mock<ITokensProvider>();
        tokensProviderMock.Setup(t => t.GenerateToken(user.Id)).Returns("token");
        var usersService = new UsersService(usersRepositoryMock.Object, mapperMock.Object, unitOfWorkMock.Object, passwordsProviderMock.Object, tokensProviderMock.Object);
        
        // Act
        var result = await usersService.LoginAsync(user.Login, user.Password);
        
        // Assert
        Assert.Equal("token", result.Token);
        usersRepositoryMock.Verify(r => r.GetByLoginAsync(user.Login), Times.Once);
    }

    [Fact]
    public async Task LoginUserIncorrectPasswordTest()
    {
        // Arrange
        var user = DataGenerator.RandomUser();
        var usersRepositoryMock = new Mock<IUsersRepository>();
        usersRepositoryMock.Setup(r => r.GetByLoginAsync(user.Login)).ReturnsAsync(user);
        var mapperMock = new Mock<IMapper>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordsProviderMock = new Mock<IPasswordsProvider>();
        passwordsProviderMock.Setup(p => p.VerifyPassword(user.Password, user.Password)).Returns(false);
        var tokensProviderMock = new Mock<ITokensProvider>();
        var usersService = new UsersService(usersRepositoryMock.Object, mapperMock.Object, unitOfWorkMock.Object, passwordsProviderMock.Object, tokensProviderMock.Object);
        
        // Act
        await Assert.ThrowsAsync<IncorrectPasswordException>(() => usersService.LoginAsync(user.Login, user.Password));
        
        // Assert
        usersRepositoryMock.Verify(r => r.GetByLoginAsync(user.Login), Times.Once);
        passwordsProviderMock.Verify(p => p.VerifyPassword(user.Password, user.Password), Times.Once);
    }

    [Fact]
    public async Task LoginUserNotFoundTest()
    {
        // Arrange
        var user = DataGenerator.RandomUser();
        var usersRepositoryMock = new Mock<IUsersRepository>();
        usersRepositoryMock.Setup(r => r.GetByLoginAsync(user.Login)).ReturnsAsync(null as User);
        var mapperMock = new Mock<IMapper>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var passwordsProviderMock = new Mock<IPasswordsProvider>();
        var tokensProviderMock = new Mock<ITokensProvider>();
        var usersService = new UsersService(usersRepositoryMock.Object, mapperMock.Object, unitOfWorkMock.Object, passwordsProviderMock.Object, tokensProviderMock.Object);
        
        // Act
        await Assert.ThrowsAsync<LoginNotFoundException>(() => usersService.LoginAsync(user.Login, user.Password));
        
        // Assert
        usersRepositoryMock.Verify(r => r.GetByLoginAsync(user.Login), Times.Once);
    }
}