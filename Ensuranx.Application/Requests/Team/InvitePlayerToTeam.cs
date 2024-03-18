namespace Ensuranx.Application.Requests.Team
{
    public class InvitePlayerToTeam
    {
        public long PlayerId { get; set; } = default!;
        public long TeamId { get; set; }
    }
}
