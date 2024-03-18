namespace Ensuranx.Application.Response.Tournament;

public class GetTournamentRoundPlayoffs
{
    public string Round { get; set; } = default!;
    public List<PlayoffGroups> PlayoffGroupList { get; set; }
}
