namespace Ensuranx.Application.Requests.Team
{
    public class AddPlayerToTheTeam
    {
        public long TeamId { get; set; }
        public long PlayerId { get; set; }
        public long EventCollectionId { get; set; }
    }
}
