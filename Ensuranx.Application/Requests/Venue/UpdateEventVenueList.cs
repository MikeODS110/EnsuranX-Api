using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Requests.Venue
{
    public class UpdateEventVenueListReq
    {
        public long Id { get; set; }
        public long StatsKeeperId { get; set; }
        public long activityId { get; set; }

    }
}
