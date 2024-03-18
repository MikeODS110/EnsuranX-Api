using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers;

public class EventCollectionDeviceMapper
{
    public EventCollectionDevice MapEventCollectionDeviceForCreate(long eventCollectionId,long deviceId,string email)
    {
        EventCollectionDevice eventCollectionDevice = new EventCollectionDevice();

        eventCollectionDevice.EventCollectionId = eventCollectionId;
        eventCollectionDevice.DeviceId = deviceId;
        eventCollectionDevice.CreatedBy = email;
        eventCollectionDevice.LastModifiedBy = email;
        eventCollectionDevice.LastModifiedDateTime = DateTime.UtcNow;
        eventCollectionDevice.CreatedDateTime = DateTime.UtcNow;

        return eventCollectionDevice;
    }
}
