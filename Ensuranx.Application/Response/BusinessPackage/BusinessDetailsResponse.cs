namespace Ensuranx.Application.Response.BusinessPackage
{
    public class BusinessDetailsResponse
    {
        public long totalVenues { get; set; }
        public long totalSports { get; set; }
        public long totalEvents { get; set; }
        public BranchDetailsModel BranchDetailsModel1 { get; set; }
        public List<BranchSchedulingDetails> BranchSchedulingDetails { get; set; }
    }

    public class BranchDetailsModel
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string Profile { get; set; }
        public long ProfileId { get; set; }
        public string Cover { get; set; }
        public long CoverId { get; set; }
    }

    public class BranchSchedulingDetails
    {
        public long Id { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string WeekDays { get; set; }
    }
}
