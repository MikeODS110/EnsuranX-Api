using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Venue
{
    public class StatkeeperInVenue
    {
        public long venueId { get; set; } = 0;
        public long VenueStatkeeper { get;set; }= 0;
        public long Venue { get; set; }
        public long UserId { get; set;}
        public long User { get; set;}
        public long? StatKeeperId { get; set; }
        public long? EC { get; set; }
    }
}
