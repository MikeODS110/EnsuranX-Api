using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Branch
{
    public class GetAllTeamsByVenueId
    {
        public long Id { get; set; }
        public long EventCollectionId { get;set; }
        public string Name { get; set; }
        public int Members { get; set; }
    }
}
