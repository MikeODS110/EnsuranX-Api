using Ensuranx.Application.Response.Branch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Venue
{
   public  class VenueByBrandIdResp
    {
        public long Id { get; set; }
        public string Name { get; set; } 

        public DateTime CreatedDate { get; set; }

        public string CurrentActivity { get; set; }

        public string CurrentActivityImageUrl { get; set; } 
        public List<StatkeeperImagesDetails> StatkeeperDetails { get; set; } = new List<StatkeeperImagesDetails>();
        public long PlayerCount { get; set; }
    }
    public class currentActivityResp
    {
        public long venueId { get; set; }
        public long eventId { get; set; }
        public long currentActId { get; set; }
        public string currentActName { get;set; }
        public string CurrentActivityImageUrl { get; set; }
    }

}
