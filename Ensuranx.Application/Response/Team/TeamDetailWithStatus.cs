namespace Ensuranx.Application.Response.Team;

public class TeamDetailWithStatus
{
    public string Name { get; set; } = string.Empty;
    public long Id { get; set; }
    public string Status { get; set; } = string.Empty;
}
