using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Requests.Venue
{
    public class UpdateVenueReq
    {
        public long BranchId { get; set; }
        public long VenueId { get; set; }
        public string Name { get; set; }
        public List<long> StatkeeperIdList { get; set; }
        public List<long> ActivityIdList { get; set; }
    }
}
