using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class ConnectionTypeRepository : IConnectionTypeRepository
    {
        private readonly ApplicationDbContext _context;
        public ConnectionTypeRepository(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<ConnectionType> GetConnectionTypeAsync(string Entity)
        {
            return await _context.ConnectionType.AsNoTracking().Where(x => x.Name.ToLower() == Entity.ToLower()).FirstOrDefaultAsync();
        }
    }
}
