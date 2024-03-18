namespace Ensuranx.Application.Requests.Event
{
    public class CreateEventCollection
    {
        public long VenueId { get; set; }
        public long ActivityCategoryId { get; set; }
        public long EventTypeId { get; set; }
        public string DeviceId { get; set; } = string.Empty;
    }
}
