namespace Ensuranx.Application.Response.Event
{
    public class EventResponse
    {
        public long EventId { get; set; }
        public bool InProgress { get; set; } = false;
        public bool IsTimeBased { get; set; } = true;
        public double EventDurationInMinutes { get; set; }
        public double EventDurationInSeconds { get; set; }
        public int TotalEventDurationInMinutes { get; set; }
        public double ShotClockTimer { get; set; } = default!;
        public bool IsEventPause { get; set; } = false;
        public string Message { get; set; }
    }
}
