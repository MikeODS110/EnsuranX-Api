using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class VenueActivityRepository : IVenueActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public VenueActivityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<VenueActivity>> AddVenueActivityList(List<VenueActivity> venueActivityList)
        {
            await _context.VenueActivity.AddRangeAsync(venueActivityList);
            await _context.SaveChangesAsync();
            return venueActivityList;
        }

        public async Task<VenueActivity> GetVenueActivityByVenueIdAndActivityId(long venueId, long activityId)
        {
            return await _context.VenueActivity.AsNoTracking().Where(x => x.VenueId == venueId && x.ActivityId == activityId).FirstOrDefaultAsync();
        }

        
    }
}
