using Ensuranx.Application.Response.Team;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Response.Branch
{
    public class BranchesVenuesResponse
    {
       

        public long Id { get; set; }

        public string VenueName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;    
        public long venueId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public double EventDuration { get; set; } = default!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime EventDate { get; set; }
        public string Status { get; set; } 
        public string CurrentActivity { get; set; } = string.Empty;
        public string StatskeeperName { get; set; } = string.Empty;
        public string StatkeeperImage { get; set; } 
        public string ActivityIcon { get; set; } 
        public List<TeamDetail> Teams { get; set; }
    }
}
