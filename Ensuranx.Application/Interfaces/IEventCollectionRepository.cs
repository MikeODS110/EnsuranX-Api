using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IEventCollectionRepository
    {
        public Task<EventCollection> GetEventCollectionList(long venueId, long activityId);
        public Task<EventCollection> GetEventCollectionByDeviceId(long deviceId);
        public Task<List<EventCollection>> CreateEventCollectionList(List<EventCollection> eventCollectionList);
        public Task<EventCollection> GetEventCollectionByDeviceIdNotLocked(long deviceId);
        public Task<EventCollection> UpdateEventCollectionMakeUnlock(long eventCollectionId);

        public Task<EventCollection> GetEventCollectionByEventCollectionId(long eventCollectionId);
        public Task<EventCollection> GetActiveEventCollectionByVenueIdActivityIdAndEventTypeId(long venueId,long activityId,long evnentTypeId);
        public Task<EventCollection> GetActiveEventCollectionByDeviceId(long deviceId);

        public Task<EventCollection> GetEventCollectionByDeviceIdWithoutLockCheck(long deviceId);
    }
}
