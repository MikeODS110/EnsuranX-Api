namespace Ensuranx.Application.Response.Event;

public class EventResultResponse
{
    public string BranchName { get; set; } = default!;
    public DateTime EventDateTime { get; set; } = default!;
    public string ActivityIcon { get; set; } = default!;
    public string StatkeeperName { get; set; } = default!;
    public string StatkeeperImage { get; set; } = default!;
    public double EventDuration { get; set; } = default!;
    public double EventDurationInMinute { get; set; } = default!;
    public double EventDurationInSecond { get; set; } = default!;
    public int TotalEventDurationInMinutes { get; set; } = default!;
    public int NumberOfShotClock { get; set; } = default!;
    public double ShotClockTimer { get; set; } = default!;
    public bool IsEventPause { get; set; } = false;
    public List<BasketballResultResponse> BasketballResultResponseList { get; set; }
}
