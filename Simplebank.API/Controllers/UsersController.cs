using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simplebank.API.Authorization;
using Simplebank.API.Exceptions;
using Simplebank.API.Requests.Users;
using Simplebank.Application.Exceptions.Users;
using Simplebank.Domain.Interfaces.Services;

namespace Simplebank.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    [Authorize]
    [Route("{id:guid}")]
    [HttpGet]
    public async Task<ActionResult> GetUserByIdAsync([FromRoute] Guid id, IUsersService usersService)
    {
        try
        {
            var user = await usersService.GetByIdAsync(id);
            return Ok(user);
        }
        catch (UserNotFoundException e)
        {
            return NotFound(ExceptionHandler.HandleException(e));
        }
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateUserAsync([FromBody] CreateUserRequest request, IUsersService usersService)
    {
        try
        {
            var user = await usersService.CreateAsync(request.Login, request.Name, request.Email, request.Password);
            return Ok(user);
        }
        catch (UserAlreadyExistsException e)
        {
            return Conflict(ExceptionHandler.HandleException(e));
        }
    }
    
    [HttpPost("login")]
    public async Task<ActionResult> LoginAsync([FromBody] LoginRequest request, IUsersService usersService)
    {
        try
        {
            var result = await usersService.LoginAsync(request.Login, request.Password);
            return Ok(result);
        }
        catch (LoginNotFoundException e)
        {
            return NotFound(ExceptionHandler.HandleException(e));
        }
        catch (IncorrectPasswordException e)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, ExceptionHandler.HandleException(e));
        }
    }
}