using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IConnectionEntityRepository
    {
        public Task<ConnectionEntity> GetConnectionEntityAsync(string Entity);
    }
}
