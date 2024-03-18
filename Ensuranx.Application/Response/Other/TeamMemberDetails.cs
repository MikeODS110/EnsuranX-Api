using Ensuranx.Common.Constants;

namespace Ensuranx.Application.Response.Other
{
    public class TeamMemberDetails
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Colour { get; set; } = string.Empty;
        public string LastPlayed { get; set; }
        public string DateFormed { get; set; }
        public int Win { get; set; }
        public int Loss { get; set; }
        public int Draw { get; set; }
        public List<MemberDetail> MemberDetailList { get; set; }
        public int ActivePlayers { get; set; }
        public int Matches { get; set; }
        public string RecentMatchActivity { get; set; }
        public string RecentMatchStatkeeper { get; set; }
        public string RecentMatchStatkeeperImage { get; set; }
        public string RecentMatchBranchName { get; set; }
        public string RecentMatchVenueName { get; set; }
    }

    public class MemberDetail
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Joined { get; set; }
        public string Left { get; set; }
        public string ProfileImage { get; set; } = Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
        public string CoverImage { get; set; } = Constants.APIErrorMessages.USER_DEFAULT_COVER_IMAGE;
        public Domain.Enums.Enum.Follows Follow { get; set; } = Domain.Enums.Enum.Follows.RequestToFollow;
    }
}
