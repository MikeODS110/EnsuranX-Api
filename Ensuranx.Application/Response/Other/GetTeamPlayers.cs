namespace Ensuranx.Application.Response.Other
{
    public class GetTeamPlayers
    {
        public long Id { get; set; }
        public string TeamName { get; set; } = default!;
        public string Colour { get; set; } = default!;
        public long Point { get; set; } = default!;
        public List<PlayerDetails> PlayerDetailList { get; set; } = default!; 
    }
}
