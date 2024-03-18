using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IConnectionRepository
    {
        public Task<List<Connection>> CreateConnectionList(List<Connection> connectionList);
        public Task<List<Connection>> GetConnectionListByConnectionEntityAndTableId(long tableId, long connectionEntityId);

        public Task<List<Connection>> UpdateConnectionList(List<Connection> connectionList);
        
    }
}
