using Ensuranx.Application.Response.Other;

namespace Ensuranx.Application.Response.User
{
    public class AllStatkeeperResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class AllStatkeepersDetails
    {
        public long UserId { get; set; }
        public string ProfileImage { get; set; }
        public string CoverImage { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }
        public string VenueName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class AllStatkeepersVenueList
    {
        public long UserId { get; set; }
        public string ProfileImage { get; set; }
        public string CoverImage { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }
        public List<GenericMessage> VenueList { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
