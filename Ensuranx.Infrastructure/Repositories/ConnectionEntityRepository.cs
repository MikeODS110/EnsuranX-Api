using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class ConnectionEntityRepository : IConnectionEntityRepository
    {
        private readonly ApplicationDbContext _context;

        public ConnectionEntityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConnectionEntity> GetConnectionEntityAsync(string Entity)
        {
            return await _context.ConnectionEntity.AsNoTracking().Where(x => x.Name.ToLower() == Entity.ToLower()).FirstOrDefaultAsync();
        }
    }
}
