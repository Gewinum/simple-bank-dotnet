using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Simplebank.API.Exceptions;
using Simplebank.API.Requests.Transfers;
using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Application.Exceptions.Transfers;
using Simplebank.Domain.Interfaces.Exceptions;
using Simplebank.Domain.Interfaces.Services;

namespace Simplebank.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TransfersController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> TransferAsync([FromServices] ITransfersService transfersService, [FromBody] TransferRequest request)
    {
        try
        {
            var result =
                await transfersService.TransferAsync(request.FromAccount, request.ToAccount, request.Amount);
            return Ok(result);
        }
        catch (InsufficientBalanceException e)
        {
            return BadRequest(ExceptionHandler.HandleException(e));
        }
        catch (AccountNotFoundException e)
        {
            return BadRequest(ExceptionHandler.HandleException(e));
        }
    }
}