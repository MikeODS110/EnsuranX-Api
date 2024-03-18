namespace Ensuranx.Application.Requests.Team
{
    public class RemoveOrBanPlayer
    {
        public bool IsRemove { get; set; } = false;
        public bool IsBan { get; set; } = false;
        public long TeamId { get; set; }
        public long PlayerId { get; set; }
    }
}
