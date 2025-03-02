using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Simplebank.API.Requests.Accounts;
using Simplebank.Application.Exceptions.Accounts;
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
            return NotFound(e.Message);
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
            return Conflict(e.Message);
        }
    }
    
    [HttpPut]
    [Route("balance")]
    public async Task<ActionResult> AddBalanceAsync([FromBody] ChangeBalanceRequest request, IAccountsService accountsService)
    {
        try
        {
            await accountsService.AddBalanceAsync(request.AccountId, request.Amount);
            return Ok();
        }
        catch (AccountNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpDelete]
    [Route("balance")]
    public async Task<ActionResult> SubtractBalanceAsync([FromBody] ChangeBalanceRequest request, IAccountsService accountsService)
    {
        try
        {
            await accountsService.SubtractBalanceAsync(request.AccountId, request.Amount);
            return Ok();
        }
        catch (AccountNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InsufficientBalanceException e)
        {
            return BadRequest(e.Message);
        }
    }
}