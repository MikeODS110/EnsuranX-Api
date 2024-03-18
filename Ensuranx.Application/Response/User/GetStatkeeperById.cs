
namespace Ensuranx.Application.Response.User
{
    public class GetStatkeeperById
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public List<GetVenueBrnach> VenueIdList { get; set; }
    }
    public class GetVenueBrnach 
    {
       // public long BranchId { get; set; }
        public long VenueId { get; set; }
        public string VenueName { get; set; }

    }
}
