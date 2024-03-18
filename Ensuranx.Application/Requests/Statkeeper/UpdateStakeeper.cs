using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Requests.Statkeeper
{
    public class UpdateStakeeper
    {
        public long StatkeeperId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public List<long> BranchId { get; set; }
        public List<long> VenueId { get; set; }

    }

}
