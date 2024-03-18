using Ensuranx.Application.Response.Tournament;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class EventCollectionMapper
    {
        public List<EventCollection> MapEventCollectionForCreate(List<long> venueIdList,long activityId, long eventTypeId,string email,long userId)
        {
            List<EventCollection> eventCollectionList = new List<EventCollection>();

            foreach (long venueId in venueIdList)
            {
                EventCollection eventCollection = new EventCollection();

                eventCollection.StatKeeperId = userId;
                eventCollection.CreatedDateTime = DateTime.UtcNow;
                eventCollection.LastModifiedDateTime = DateTime.UtcNow;
                eventCollection.LastModifiedBy = email;
                eventCollection.CreatedBy = email;
                eventCollection.VenueId = venueId;
                eventCollection.ActivityId = activityId;
                eventCollection.EventTypeId = eventTypeId;

                eventCollectionList.Add(eventCollection);
            }
            

            return eventCollectionList;
        }

        public List<EventCollection> MapEventCollectionForCreate(List<MatchUpDetails> matchUpDetailList, long activityId, long eventTypeId, string email, long userId)
        {
            List<EventCollection> eventCollectionList = new List<EventCollection>();

            foreach (MatchUpDetails matchUpDetail in matchUpDetailList)
            {
                EventCollection eventCollection = new EventCollection();

                eventCollection.StatKeeperId = matchUpDetail.StatkeeperId;
                eventCollection.CreatedDateTime = DateTime.UtcNow;
                eventCollection.LastModifiedDateTime = DateTime.UtcNow;
                eventCollection.LastModifiedBy = email;
                eventCollection.CreatedBy = email;
                eventCollection.VenueId = matchUpDetail.VenueId;
                eventCollection.ActivityId = activityId;
                eventCollection.EventTypeId = eventTypeId;

                eventCollectionList.Add(eventCollection);
            }

            return eventCollectionList;
        }

        public EventCollection MapEventCollectionForCreate(long venueId, long activityId, long eventTypeId, string email, long userId)
        {
            EventCollection eventCollection = new EventCollection();

            eventCollection.StatKeeperId = userId;
            eventCollection.CreatedDateTime = DateTime.UtcNow;
            eventCollection.LastModifiedDateTime = DateTime.UtcNow;
            eventCollection.LastModifiedBy = email;
            eventCollection.CreatedBy = email;
            eventCollection.VenueId = venueId;
            eventCollection.ActivityId = activityId;
            eventCollection.EventTypeId = eventTypeId;
           

            return eventCollection;
        }

        public Device InitiateDevice(string email,string deviceToken)
        {
            Device device = new Device();


            device.DeviceToken = deviceToken;
            device.CreatedDateTime = DateTime.UtcNow;
            device.LastModifiedDateTime = DateTime.UtcNow;
            device.LastModifiedBy = email;
            device.CreatedBy = email;


            return device;

        }
    }
}
