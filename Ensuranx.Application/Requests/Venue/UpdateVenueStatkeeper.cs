using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Requests.Venue
{
    public class UpdateVenueStatkeeper
    {
        public long Id { get; set; }
        public long StatsKeeperId { get; set; }
        public string StatkeeperContact { get; set; }
        public long activityId { get; set; }
        public long venueId { get; set; }
    }
}
