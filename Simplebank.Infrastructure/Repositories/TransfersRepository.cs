using Simplebank.Domain.Database.Models;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Infrastructure.Database;

namespace Simplebank.Infrastructure.Repositories;

public class TransfersRepository : Repository<Transfer>, ITransfersRepository
{
    public TransfersRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}