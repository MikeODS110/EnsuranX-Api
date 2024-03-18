using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly ApplicationDbContext _context;
        public ConnectionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Connection>> CreateConnectionList(List<Connection> connectionList)
        {
            await _context.Connection.AddRangeAsync(connectionList);
            await _context.SaveChangesAsync();
            return connectionList;
        }

        public async Task<List<Connection>> GetConnectionListByConnectionEntityAndTableId(long tableId,long connectionEntityId)
        {
            return await _context.Connection.AsNoTracking().Where(x => x.TableId == tableId && x.ConnectionEntityId == connectionEntityId).ToListAsync();
        }

        public async Task<List<Connection>> UpdateConnectionList(List<Connection> connectionList)
        {
            _context.Connection.UpdateRange(connectionList);
            await _context.SaveChangesAsync();
            return connectionList;
        }
    }
}
