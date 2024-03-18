namespace Ensuranx.Application.Response.Team;

public class GetRecentTeam
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Colour { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string RecentMatchActivity { get; set; } = string.Empty;
    public string RecentMatchBranchName { get; set; } = string.Empty;
    public string RecentMatchVenueName { get; set; } = string.Empty;
    public string RecentMatchStatkeeper { get; set; } = string.Empty;
    public string RecentMatchStatkeeperImage { get; set; } = string.Empty;
    public int ActivePlayers { get; set; } = 0;
}
