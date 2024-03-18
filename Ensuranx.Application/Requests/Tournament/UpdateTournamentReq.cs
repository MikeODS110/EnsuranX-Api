using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Requests.Tournament
{
    public class UpdateTournamentReq
    {
        public long Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Name { get; set; }
        public long ActivityId { get; set; }
        public List<long> VenueIdList { get; set; }
        public List<long> StatkeeperIdList { get; set; }
        public int TeamCapacity { get; set; }
        public int MinPlayerPerTeam { get; set; }
        public int MaxPlayerPerTeam { get; set; }
        public long FirstPrize { get; set; }
        public long SecondPrize { get; set; }
        public long ThirdPrize { get; set;}
        public DateTime SeasonStartDate { get; set; }
        public DateTime SeasonEndDate { get; set; }
        public int SeasonTeamCapacity { get; set; }
    }
}
