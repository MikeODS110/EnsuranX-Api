using Ensuranx.Application.Response.Branch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Event
{
    public class TeamScorePoints
    {

        public long eventLogId { get; set; }
        public long playerId { get; set; }
        public long teamId { get;set;} 
        public int score { get;set;}
    }
}
