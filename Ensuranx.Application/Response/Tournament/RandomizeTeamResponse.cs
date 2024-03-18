namespace Ensuranx.Application.Response.Tournament;

public class RandomizeTeamResponse
{
    public List<long> EventTeamIdList { get; set; }
    public long VenueId { get; set; }
    public long StatkeeperId { get; set; }
    public DateTime EventDateTime { get; set; }
}
