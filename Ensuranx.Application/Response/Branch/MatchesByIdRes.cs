using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Branch
{
    public class MatchesByIdRes
    {
        public long Id { get; set; }
        public long venueId { get; set; }
        public string statKeeperName { get; set; }
        public string activity { get; set; }
    }
}
