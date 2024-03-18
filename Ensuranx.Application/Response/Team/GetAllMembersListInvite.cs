namespace Ensuranx.Application.Response.Team;

public class GetAllMembersListInvite
{
    public List<GetInviteMemberList> Invited { get; set; }
    public List<GetInviteMemberList> Friends { get; set; }
    public List<GetInviteMemberList> RecentlyPlayedWith { get; set; }
}
