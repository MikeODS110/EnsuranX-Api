using Ensuranx.Application.Response.Team;

namespace Ensuranx.Application.Response.Tournament;

public class TournamentPlayoffResponse
{
    public long EventId { get; set; }
    public int Round { get; set; }
    public string Group { get; set; } = string.Empty;
    public List<TeamDetail> TeamDetailList { get; set; }
}
