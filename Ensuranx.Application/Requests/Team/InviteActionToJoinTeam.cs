using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Application.Requests.Team
{
    public class InviteActionToJoinTeam
    {
        public long PlayerId { get; set; }
        public long TeamId { get; set; }
        public InviteAction InviteAction { get; set; }
    }
}
