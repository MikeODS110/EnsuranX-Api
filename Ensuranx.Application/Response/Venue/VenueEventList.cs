using Ensuranx.Application.Response.Branch;
using System.Net.Sockets;

namespace Ensuranx.Application.Response.Venue
{
    public class VenueEventListResponse
    {
        public long Id { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public long venueId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public string CurrentActivity { get; set; } = string.Empty;
        public string ActivityIcon { get; set; } = string.Empty;
        public string StatskeeperName { get; set; } = string.Empty;
        public string StatskeeperImage { get; set; } = string.Empty;
        public string StatskeeperContact { get; set; } = string.Empty;
        public List<GetAllPlayersByEC> AllPlayers { get; set; } 
        public long ActivePlayers { get; set; } 
    }
}
