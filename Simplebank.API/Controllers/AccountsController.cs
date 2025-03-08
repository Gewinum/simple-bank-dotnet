using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Simplebank.API.Exceptions;
using Simplebank.API.Requests.Accounts;
using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Domain.Interfaces.Exceptions;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;
using Simplebank.Domain.Models.Users;

namespace Simplebank.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountsController : ControllerBase
{
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<ActionResult> GetAccountByIdAsync(Guid id, IAccountsService accountsService)
    {
        try
        {
            var account = await accountsService.GetAccountAsync(id);
            return Ok(account);
        }
        catch (AccountNotFoundException e)
        {
            return NotFound(ExceptionHandler.HandleException(e));
        }
    }
    
    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateAccountAsync([FromBody] CreateAccountRequest request, IAccountsService accountsService)
    {
        try
        {
            var account = await accountsService.CreateAccountAsync(request.Owner, request.Currency);
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
        try
        {
            var entry = await accountsService.AddBalanceAsync(request.AccountId, request.Amount);
            return Ok(entry);
        }
        catch (AccountNotFoundException e)
        {
            return NotFound(ExceptionHandler.HandleException(e));
        }
    }
}