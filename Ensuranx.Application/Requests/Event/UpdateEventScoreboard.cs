namespace Ensuranx.Application.Requests.Event
{
    public class UpdateEventScoreboard
    {
        public long EventId { get; set; }
        public long TeamMemberId { get; set; }
        public long StatId { get; set; }
        public bool IsSucessfull { get; set; } = true;
        public bool IsFix { get; set; } = false;
        public int IsPause { get; set; } = 0;
        public bool IsShotClock { get; set; } = false;
    }
}
