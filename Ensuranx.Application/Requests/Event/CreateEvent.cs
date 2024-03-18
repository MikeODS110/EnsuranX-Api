namespace Ensuranx.Application.Requests.Event
{
    public class CreateEvent
    {
        public List<long> teamIdList { get; set; }
        public long EventId { get; set; } = 0;
    }
}
