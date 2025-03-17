using Simplebank.Domain.Models.Transfers;

namespace Simplebank.Domain.Interfaces.Services;

public interface ITransfersService
{
    Task<TransferResult> TransferAsync(Guid userId, Guid fromAccountId, Guid toAccountId, decimal amount);
}