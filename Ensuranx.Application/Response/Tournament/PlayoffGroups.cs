namespace Ensuranx.Application.Response.Tournament;

public class PlayoffGroups
{
    public string RoundName { get; set; } = default!;
    public List<TournamentPlayoffResponse> tournamentPlayoffResponseList { get; set; }
}
