using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IVenueStatkeeperRepository
    {
        public Task<List<VenuesStatskeeper>> AddVenueStatkeeperList(List<VenuesStatskeeper> venuesStatskeeperList);
        public Task<List<VenuesStatskeeper>> GetVenuesStatskeeperListByUserId(int userId);
        
    }
}
