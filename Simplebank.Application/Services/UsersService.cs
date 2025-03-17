using AutoMapper;
using Simplebank.Application.Exceptions.Users;
using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Database;
using Simplebank.Domain.Interfaces.Providers;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;
using Simplebank.Domain.Models.Users;
using Simplebank.Infrastructure.Exceptions;

namespace Simplebank.Application.Services;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordsProvider _passwordsProvider;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokensProvider _tokensProvider;
    
    public UsersService(IUsersRepository usersRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IPasswordsProvider passwordsProvider,
        ITokensProvider tokensProvider)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _passwordsProvider = passwordsProvider;
        _tokensProvider = tokensProvider;
    }
    
    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _usersRepository.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new UserNotFoundException(id);
        }
        
        return _mapper.Map<UserDto>(user);
    }
    
    public async Task<UserDto> CreateAsync(string login, string name, string email, string password)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            if (await _usersRepository.GetByLoginAsync(login) != null)
            {
                throw new UserAlreadyExistsException(login, email);
            }
            if (await _usersRepository.GetByEmailAsync(email) != null)
            {
                throw new UserAlreadyExistsException(login, email);
            }
            
            var user = new User
            {
                Login = login,
                Name = name,
                Email = email,
                Password = _passwordsProvider.CreateHash(password)
            };
            await _usersRepository.AddAsync(user);
            await _unitOfWork.CommitTransactionAsync();
            return _mapper.Map<UserDto>(user);
        }
        catch (DuplicateKeysException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new UserAlreadyExistsException(login, email);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw e.InnerException ?? e;
        }
    }

    public async Task<LoginResult> LoginAsync(string login, string password)
    {
        var user = await _usersRepository.GetByLoginAsync(login);
        if (user == null)
        {
            throw new LoginNotFoundException(login);
        }

        if (!_passwordsProvider.VerifyPassword(password, user.Password))
        {
            throw new IncorrectPasswordException(login);
        }

        var token = _tokensProvider.GenerateToken(user.Id);
        return new LoginResult
        {
            Token = token,
        };
    }
}