using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Response.Team
{
    public class GetAllAvgResp
    {
        public decimal AvgWinPerc { get; set; }
        public decimal AvgWinScore{ get; set; }
        public decimal AvgTotalScore { get; set; }
        public decimal AvgAssit { get; set; }
        public decimal AvgBlock { get; set; }
        public decimal AvgMvpPerc{ get; set; }
        public decimal AvgRebound { get; set; }
        public decimal AvgStealWinPercent { get; set; }
        public decimal AvgTotalPoint { get; set; }
        public decimal AvgSteal { get; set; }
    }
}
