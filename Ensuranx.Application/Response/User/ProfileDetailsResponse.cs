namespace Ensuranx.Application.Response.User
{
    public class ProfileDetailsResponse
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public long Followers { get; set; } = 0;
        public long Following { get; set; } = 0;
        public DateTime LastGame { get; set; }
        public DateTime LastMvp { get; set; }
        public long Matches { get; set; } = 0;
        public long Win { get; set; } = 0;
        public long Lose { get; set; } = 0;
        public long Tie { get; set; } = 0;
        public long OfficialTeam { get; set; } = 0;
        public long TeamporaryTeam { get; set; } = 0;
        public long Venue { get; set; } = 0;
        public long TournamentParticipated { get; set; } = 0;
        public long TournamentWin { get; set; } = 0;
        public long TournamentLose { get; set; } = 0;
        public string ProfileImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public string JerseyNumber {get;set;}
        public Domain.Enums.Enum.Follows Follow { get; set; } = Domain.Enums.Enum.Follows.RequestToFollow;
    }
}
