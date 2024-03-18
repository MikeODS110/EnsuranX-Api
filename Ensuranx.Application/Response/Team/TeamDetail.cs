namespace Ensuranx.Application.Response.Team;

public class TeamDetail
{
    public long eventid { get; set; }
    public string Name { get; set; } = string.Empty;
    public long Id { get; set; }
    public long Points { get; set; } = default!;
    public int Round { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Colour { get; set; } = default!;
}
