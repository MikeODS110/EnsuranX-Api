namespace Ensuranx.Application.Response.Team;

public class GetInviteMemberList
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Path { get; set; } = default!;
    public bool IsInvite { get; set; } = false;
}
