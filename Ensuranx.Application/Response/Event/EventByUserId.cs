using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Event
{
    public class EventByUserId
    {
        public long eventId { get; set; }
        public long FirstTeamId { get;set; }
        public long LastTeamId { get;set; }
    }
}
