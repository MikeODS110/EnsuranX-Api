using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IConnectionTypeRepository
    {
        public Task<ConnectionType> GetConnectionTypeAsync(string Entity);
    }
}
