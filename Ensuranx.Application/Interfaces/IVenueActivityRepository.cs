using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IVenueActivityRepository
    {
        public Task<List<VenueActivity>> AddVenueActivityList(List<VenueActivity> venueActivityList);
        public Task<VenueActivity> GetVenueActivityByVenueIdAndActivityId(long venueId, long activityId);
        //public Task<Ensuranx.Domain.Entities.EventCollection> UpdateEventCollection(Ensuranx.Domain.Entities.EventCollection EventCollection);

    }
}
