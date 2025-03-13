using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Simplebank.API.Authorization;
using Simplebank.API.Exceptions;
using Simplebank.API.Requests.Accounts;
using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Domain.Interfaces.Exceptions;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;
using Simplebank.Domain.Models.Users;

namespace Simplebank.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AccountsController : ControllerBase
{
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<ActionResult> GetAccountByIdAsync(Guid id, IAccountsService accountsService)
    {
        var tokenInfo = UserInfoExtractor.ExtractFromRequest(User);
        try
        {
            var account = await accountsService.GetAccountAsync(tokenInfo.UserId, id);
            return Ok(account);
        }
        catch (AccountNotFoundException e)
        {
            return NotFound(ExceptionHandler.HandleException(e));
        }
        catch (AccountNotOwnedException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ExceptionHandler.HandleException(e));
        }
    }

    [HttpGet]
    public async Task<ActionResult> GetAccountsAsync(IAccountsService accountsService)
    {
        var tokenInfo = UserInfoExtractor.ExtractFromRequest(User);
        var accounts = await accountsService.GetAccountsAsync(tokenInfo.UserId);
        return Ok(accounts);
    }
    
    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateAccountAsync([FromBody] CreateAccountRequest request, IAccountsService accountsService)
    {
        var tokenInfo = UserInfoExtractor.ExtractFromRequest(User);
        try
        {
            var account = await accountsService.CreateAccountAsync(tokenInfo.UserId, request.Currency);
            return Ok(account);
        }
        catch (AccountAlreadyExistsException e)
        {
            return Conflict(ExceptionHandler.HandleException(e));
        }
    }
    
    [HttpPost]
    [Route("balance")]
    public async Task<ActionResult> AddBalanceAsync([FromBody] ChangeBalanceRequest request, IAccountsService accountsService)
    {
        var tokenInfo = UserInfoExtractor.ExtractFromRequest(User);
        try
        {
            var entry = await accountsService.AddBalanceAsync(tokenInfo.UserId, request.AccountId, request.Amount);
            return Ok(entry);
        }
        catch (AccountNotFoundException e)
        {
            return NotFound(ExceptionHandler.HandleException(e));
        }
        catch (InsufficientBalanceException e)
        {
            return BadRequest(ExceptionHandler.HandleException(e));
        }
        catch (AccountNotOwnedException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ExceptionHandler.HandleException(e));
        }
    }
}