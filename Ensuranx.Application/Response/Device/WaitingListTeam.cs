namespace Ensuranx.Application.Response.Device;

public class WaitingListTeam
{
    public long TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public List<GetWaitingList> getWaitingList { get; set; }
}
