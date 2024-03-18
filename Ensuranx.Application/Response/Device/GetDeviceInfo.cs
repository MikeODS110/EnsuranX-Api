namespace Ensuranx.Application.Response.Device
{
    public class GetDeviceInfo
    {
        public long EventTypeId { get; set; }
        public string EventTypeName { get; set; } = string.Empty;
        public long VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public long ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
    }
}
