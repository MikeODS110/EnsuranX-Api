using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class VenueActivityMapper
    {
        public List<VenueActivity> MapVenueWithStatkeeperList(string email, long venueId, List<long> ActivityIdList, DateTime createdDatetime)
        {
            List<VenueActivity> venueActivityList = new List<VenueActivity>();

            foreach (var Activity in ActivityIdList)
            {
                VenueActivity venueActivity = new VenueActivity();

                venueActivity.ActivityId = Activity;
                venueActivity.VenueId = venueId;
                venueActivity.CreatedBy = email;
                venueActivity.LastModifiedBy = email;
                venueActivity.CreatedDateTime = createdDatetime;
                venueActivity.LastModifiedDateTime = createdDatetime;

                venueActivityList.Add(venueActivity);
            }

            return venueActivityList;
        }
    }
}
