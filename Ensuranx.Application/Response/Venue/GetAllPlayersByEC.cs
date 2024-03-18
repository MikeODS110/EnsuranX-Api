using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Venue
{
    public class GetAllPlayersByEC
    {
        public long Id { get; set; }
        public long PlayerCount { get; set; }
        public long TeamCount { get;set; }
    }
}
