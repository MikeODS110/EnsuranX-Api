using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Event
{
    public class MatchSummaryResp
    {
        public DateTime eventStartDateTime { get; set; }
        public long Quarters { get; set; }
        public List<MatchSummaryRespList> MatchSummaryRespLists { get;set; }
    }
    public class MatchSummaryRespList
    {
        public long playerId { get; set; }
        public long teamId { get; set; }
        public string teamColor { get; set; }
        public string playerName { get; set; }
        public string playerImage { get; set; }
        public string teamName { get; set; }

        public bool isSucessfull { get; set; }
        public string stateName { get; set; }

        public DateTime logDateTime { get; set; }

        public long teamOneScore { get; set; }
        public long teamTwoScore { get; set;}

    }

}
