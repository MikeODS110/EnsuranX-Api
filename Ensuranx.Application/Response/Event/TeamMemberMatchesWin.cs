using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Response.Event;

public class TeamMemberMatchesWin
{
    public TeamMember TeamMember { get; set; }
    public EventTeam EventTeam { get; set; }
}
