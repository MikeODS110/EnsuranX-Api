using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.User
{
    public class SubstitutePlayer
    {
        public long teamId { get; set; }
        public long eventId { get; set; }
        public long substituteId { get; set; }
        public long substituteTo { get; set; }
    }
}
