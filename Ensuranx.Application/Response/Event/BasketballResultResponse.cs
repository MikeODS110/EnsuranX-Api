namespace Ensuranx.Application.Response.Event
{
    public class BasketballResultResponse
    {
        public long TeamId { get; set; }
        public string TeamName { get; set; } = default!;
        public string EventStatus { get; set; } = default!;
        public decimal Points { get; set; } = default!;
        public string Colour { get; set; } = default!;
        public List<BasketballPlayerMatchSummary> basketballPlayerMatchSummaryList { get; set; }
    }

    public class BasketballPlayerMatchSummary
    {
        public long Id { get; set; }
        public string PlayerName { get; set; } = default!;
        public string PlayerRole { get; set; } = default!;
        public bool IsSubstituteOrDisqualify { get; set; } = false;
        public string SequenceNumber { get; set; } = default!;
        public long Pts { get; set; }
        public long Ast { get; set; } 
        public long Rebs { get; set; }
        public long Stls { get; set; }
        public long Blks { get; set; }
        public decimal WinScore { get; set; } = default!;
        public decimal TotalScore { get; set; } = default!;
        public string MvpPercentage { get; set; } = default!;
        public string WinPercentage { get; set; } = default!;
        public bool IsMvp { get; set; } = false;
        public long MvpCount { get; set; } = 0;
        public long Gp { get; set; } = 0;
        public string ProfileImage { get; set; } = string.Empty;
    }
}
