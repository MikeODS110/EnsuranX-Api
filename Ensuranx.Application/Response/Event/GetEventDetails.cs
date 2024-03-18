namespace Ensuranx.Application.Response.Event;

public class GetEventDetails
{
    public long EventId { get; set;}
    public string ActivityIcon { get; set;} = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public DateTime EventDateTime { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public string StatkeeperName { get; set; } = string.Empty;
    public string StatkeeperImage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
