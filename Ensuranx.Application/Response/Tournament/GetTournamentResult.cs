namespace Ensuranx.Application.Response.Tournament;

public class GetTournamentResult
{
    public string TeamName { get; set; } = default!;
    public string Colour { get; set; } = default!;
    public int TeamPlayerCount { get; set; }
    public int Win { get; set; }
    public int lose { get; set; }
    public int Tie { get; set; }
    public string Position { get; set; }
}
