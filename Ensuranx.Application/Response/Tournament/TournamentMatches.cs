namespace Ensuranx.Application.Response.Tournament
{
    public class TournamentMatches
    {
        public string Name { get; set; }
        public List<GenericObj> TeamList { get; set; }
        public DateTime MatchDateTime { get; set; }
        public string VenueName { get; set; }
    }
}
