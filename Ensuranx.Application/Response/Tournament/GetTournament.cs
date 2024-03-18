namespace Ensuranx.Application.Response.Tournament
{
    public class GetTournament
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }
        public List<VenueListWithBranch> VenueList { get; set; }
        public List<GenericObj> StatKeeperList { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string ActivityName { get; set; }
        public long ActivityId { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedByImage { get; set; }
        public int? TeamCapacity { get; set; }
        public int? TeamJoinedCount { get; set; }
        public int? MinPlayerPerTeam { get; set; }
        public int? MaxPlayerPerTeam { get; set; }
        public string Status { get; set; }
        public string JoinedStatus { get; set; } = string.Empty;
        public bool IsCaptain { get; set; } = false;
        public long FirstPrize { get; set; }
        public long SecondPrize { get; set;}
        public long ThirdPrize { get;set; }
        public DateTime SeasonStartDate { get; set; }
        public DateTime SeasonEndDate { get; set; }
        public int SeasonTeamCapacity { get; set; }

    }
    public class VenueListWithBranch
    {
        public long BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string VenueName { get; set; } = string.Empty;
        public long VenueId { get; set; }
    }
    public class GenericObj
    {
        public string Name { get; set; } = string.Empty;
        public long Id { get; set; }
    }
}
