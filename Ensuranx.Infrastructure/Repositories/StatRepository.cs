using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class StatRepository : IStatRepository
    {
        private readonly ApplicationDbContext _context;

        public StatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Stat>> GetActivityStatListAsync(long activityId)
        {
            var query = await _context.Stat.AsNoTracking().Where(x => x.ActivityId == activityId)
                        .Include(x => x.Activity)
                        .Include(x => x.StatType)
                        .ToListAsync();
            return query;
        }
    }
}
