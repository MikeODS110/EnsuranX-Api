namespace Ensuranx.Application.Response.Team
{
    public class GetAllInvites
    {
        public long PlayerId { get; set; }
        public string PlayerName { get; set; } = default!;
        public string PlayerImage { get; set; } = default!;
    }
}
