namespace Ensuranx.Application.Requests.Tournament
{
    public class CreateTournament
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime SeasonStartDate { get; set; }
        public DateTime SeasonEndDate { get; set; }
        public int SeasonTeamCapacity { get; set; }
        public string Name { get; set; }
        public List<long> BranchId { get; set; }
        public long TournamentTypeId { get; set; } = 2;
        public List<long> VenueIdList { get; set; }
        public List<long> StatkeeperIdList { get; set; }
        public long ActivityId { get; set; }
        public long? FirstPrize { get; set; }
        public long? SecondPrize { get; set; }
        public long? ThirdPrize { get; set; }
        public int TeamCapacity { get; set; }
        public int MinPlayerPerTeam { get; set; }
        public int MaxPlayerPerTeam { get; set; }
    }
}
