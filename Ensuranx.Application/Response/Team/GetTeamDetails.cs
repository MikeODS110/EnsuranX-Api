namespace Ensuranx.Application.Response.Team;

public class GetTeamDetails
{
    public long TeamId { get; set; }
    public string TeamName { get; set; } = default!;
    public long TeamActivity { get; set; } = default!;
    public string TeamActivityName { get; set; } = default!;
    public string Colour { get; set; } = default!;
    public int TeamMembersCount { get; set; } = 0;
    public string TeamType { get; set; } = default!;
    public DateTime DateFormed { get; set; } = default!;
    public DateTime LastPlayed { get; set; } = default!;
    public int Match { get; set; } = 0;
    public int Won { get; set; } = 0;
    public int Loss { get; set; } = 0;
    public int Tie { get; set; } = 0;
    public bool IsCaptain { get; set; } = false;
    //public List<GetEventDetails> GetEventDetailList { get; set; }
    //public List<GetAllTournament> GetAllTournamentList { get; set; }

}
