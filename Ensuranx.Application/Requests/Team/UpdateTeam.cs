namespace Ensuranx.Application.Requests.Team;

public class UpdateTeam
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public long ActivityId { get; set; }
}
