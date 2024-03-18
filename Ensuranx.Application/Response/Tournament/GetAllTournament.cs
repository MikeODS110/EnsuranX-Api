namespace Ensuranx.Application.Response.Tournament
{
    public class GetAllTournament
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int TeamCapacity { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByImage { get; set; }
        public string ActivityIcon { get; set; }
        public string ActivityName { get; set; }
        public string Status { get; set; }
        public long TeamPlayedInTournament { get; set; } = 0;
        public string JoinedStatus { get; set; } = string.Empty;
    }
}
