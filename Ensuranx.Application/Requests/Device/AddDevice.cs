namespace Ensuranx.Application.Requests.Device
{
    public class AddDevice
    {
        public string DeviceId { get; set; }
        public long VenueId { get; set; }
        public long ActivityId { get; set; }
        public long EventTypeId { get; set; }
    }
}
