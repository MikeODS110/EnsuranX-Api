namespace Ensuranx.Application.Response.Device;

public class WaitingListLoginCheck
{
    public bool isCollectionActive { get; set; } = false;
    public long activeEventCollectionId { get; set; } = 0;
    public long TournamentId { get; set; } = 0;
    public long ActiveEventId { get; set; } = 0;
    public bool IsLock { get; set; } = false;
}
