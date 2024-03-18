using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class EventCollectionRepository : IEventCollectionRepository
    {
        private readonly ApplicationDbContext _context;

        public EventCollectionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EventCollection> GetEventCollectionList(long venueId,long activityId)
        {
            return await _context.EventCollection.AsNoTracking().Where(x => x.VenueId == venueId && x.ActivityId == activityId && x.IsLocked != true).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        }

        public async Task<EventCollection> GetEventCollectionByDeviceId(long deviceId)
        {
            return await (from deviceTbl in _context.Device.AsNoTracking()
                          join eventCollectionDevice in _context.EventCollectionDevice.AsNoTracking().Include(x => x.EventCollection.Activity.ActivityCategory)
                          .Include(x => x.EventCollection.EventType)
                          .Include(x => x.EventCollection.Venue)
                          on deviceTbl.Id equals eventCollectionDevice.DeviceId

                          where deviceTbl.Id == deviceId && eventCollectionDevice.EventCollection.IsActive != false && eventCollectionDevice.EventCollection.IsDeleted != true

                          select eventCollectionDevice.EventCollection
                          ).FirstOrDefaultAsync();
        }

        public async Task<EventCollection> GetEventCollectionByEventCollectionId(long eventCollectionId)
        {
            return await _context.EventCollection.AsNoTracking()
                .Where(x => x.Id == eventCollectionId && x.IsDeleted != true && x.IsActive != false)
                .Include(x => x.Activity)
                .Include(x => x.EventType)
                .FirstOrDefaultAsync();
        }

        public async Task<EventCollection> GetEventCollectionByDeviceIdNotLocked(long deviceId)
        {
            return await (from device in _context.Device
                          join eventCollectionDevice in _context.EventCollectionDevice.Include(x => x.EventCollection.EventType)
                          on device.Id equals eventCollectionDevice.DeviceId

                          where eventCollectionDevice.DeviceId == deviceId && eventCollectionDevice.EventCollection.IsActive != false &&
                          eventCollectionDevice.EventCollection.IsDeleted != true && eventCollectionDevice.EventCollection.IsLocked != false
                          select eventCollectionDevice.EventCollection
                          ).FirstOrDefaultAsync();
        }

        public async Task<EventCollection> GetEventCollectionByDeviceIdWithoutLockCheck(long deviceId)
        {
            return await (from device in _context.Device
                          join eventCollectionDevice in _context.EventCollectionDevice.Include(x => x.EventCollection.EventType)
                          on device.Id equals eventCollectionDevice.DeviceId

                          where eventCollectionDevice.DeviceId == deviceId && eventCollectionDevice.EventCollection.IsActive != false &&
                          eventCollectionDevice.EventCollection.IsDeleted != true
                          select eventCollectionDevice.EventCollection
                          ).FirstOrDefaultAsync();
        }

        public async Task<EventCollection> UpdateEventCollectionMakeUnlock(long eventCollectionId)
        {
            var getEventCollection = await _context.EventCollection.Where(x => x.Id == eventCollectionId).FirstOrDefaultAsync();
            getEventCollection.IsLocked = false;

            _context.EventCollection.Update(getEventCollection);
            await _context.SaveChangesAsync();
            return getEventCollection;
        }

        public async Task<List<EventCollection>> CreateEventCollectionList(List<EventCollection> eventCollectionList)
        {
            await _context.EventCollection.AddRangeAsync(eventCollectionList);
            await _context.SaveChangesAsync();
            return eventCollectionList;
        }


        public async Task<EventCollection> GetActiveEventCollectionByVenueIdActivityIdAndEventTypeId(long venueId, long activityId, long evnentTypeId)
        {
            return await _context.EventCollection.AsNoTracking().Where(x => x.VenueId == venueId && x.ActivityId == activityId && x.EventTypeId == evnentTypeId
                                            && x.IsDeleted != true && x.IsActive != false).FirstOrDefaultAsync();
        }

        public async Task<EventCollection> GetActiveEventCollectionByDeviceId(long deviceId)
        {
            var query = await (from device in _context.Device
                               join eventCollectionDevice in _context.EventCollectionDevice
                               on device.Id equals eventCollectionDevice.DeviceId

                               where device.Id == deviceId && eventCollectionDevice.EventCollection.IsDeleted != true && eventCollectionDevice.EventCollection.IsActive != false

                               select eventCollectionDevice.EventCollection
                               ).FirstOrDefaultAsync();

            return query;
        }
    }
}
