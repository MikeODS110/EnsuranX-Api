namespace Ensuranx.Application.Response.Venue
{
    public class BranchesVenuesListById
    {
        public long Id { get; set; }
        public long BranchId { get; set; }
        public string VenueName { get; set; }
        public long StatsKeeperId { get; set; }
        public long ActivityId { get; set; }
    }
}
