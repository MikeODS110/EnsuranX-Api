using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Branch
{
    public class BranchVenueListWithoutTeam
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
        public string StatkeeperImage { get; set; } = string.Empty;
        public string ActivityIcon { get; set; } = string.Empty;
    }
}
