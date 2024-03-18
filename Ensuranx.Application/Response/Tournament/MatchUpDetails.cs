namespace Ensuranx.Application.Response.Tournament;

public class MatchUpDetails
{
    public List<long> TeamList { get; set; }
    public DateTime EventDateTime { get; set; }
    public long VenueId { get; set; }
    public long StatkeeperId { get; set; }
}
