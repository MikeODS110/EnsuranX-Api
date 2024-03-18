using Ensuranx.Application.Response.Event;

namespace Ensuranx.Application.Response.Team;

public class GetTeamPlayersStat
{
    public long TeamId { get; set; }
    public string TeamName { get; set; } = default!;
    public string Colour { get; set; } = default!;
    public string TeamType { get; set; } = default!;
    public DateTime DateCreated { get; set; }
    public DateTime LastPlayedDate { get; set; }
    public int PlayersCount { get; set; }
    public long Match { get; set; } = 0;
    public int Won { get; set; } = 0;
    public int Loss { get; set; } = 0;
    public int Tie { get; set; } = 0;
    public List<BasketballPlayerMatchSummary> BasketballPlayerMatchSummaryList { get; set; }
}
