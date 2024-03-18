using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Branch
{
    public class GetBranchesVenuesListWithTeams
    {
        public long Id { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public long venueId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public string CurrentActivity { get; set; } = string.Empty;
        public string StatskeeperName { get; set; } = string.Empty;
        public string StatskeeperContact { get; set; } = string.Empty;
        public List<GetAllTeamsByVenueId> ActiveTeams { get; set; }
        public int ActivePlayers { get; set; } = 0;
    }
}

