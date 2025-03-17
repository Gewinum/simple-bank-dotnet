using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simplebank.API.Authorization;
using Simplebank.API.Exceptions;
using Simplebank.API.Requests.Transfers;
using Simplebank.Application.Exceptions.Accounts;
using Simplebank.Domain.Interfaces.Services;

namespace Simplebank.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TransfersController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> TransferAsync([FromServices] ITransfersService transfersService, [FromBody] TransferRequest request)
    {
        var tokenInfo = UserInfoExtractor.ExtractFromRequest(User);
        try
        {
            var result =
                await transfersService.TransferAsync(tokenInfo.UserId, request.FromAccount, request.ToAccount, request.Amount);
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
        catch (AccountNotOwnedException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ExceptionHandler.HandleException(e));
        }
    }
}