using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class VenueStatkeeperRepository : IVenueStatkeeperRepository
    {
        private readonly ApplicationDbContext _context;

        public VenueStatkeeperRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<VenuesStatskeeper>> AddVenueStatkeeperList(List<VenuesStatskeeper> venuesStatskeeperList)
        {
            await _context.VenueStatkeeper.AddRangeAsync(venuesStatskeeperList);
            await _context.SaveChangesAsync();
            return venuesStatskeeperList;
        }

        public async Task<List<VenuesStatskeeper>> GetVenuesStatskeeperListByUserId(int userId)
        {
            return await _context.VenueStatkeeper.Where(x => x.UserInfoId == userId).ToListAsync();
        }
    }
}
