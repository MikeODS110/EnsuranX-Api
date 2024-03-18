using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Requests.Tournament
{
    public class UpdatePrizeReq
    {
        public long Id { get; set; }
        public long FirstPrize { get; set; }
        public long SecondPrize { get;set;}

        public long ThirdPrize { get; set; }
    }
}
