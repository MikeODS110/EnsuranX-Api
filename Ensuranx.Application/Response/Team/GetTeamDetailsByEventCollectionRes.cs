namespace Ensuranx.Application.Response.Team
{
    public class GetTeamDetailsByEventCollectionRes
    {
        public long teamId { get; set; }
        public string teamName { get; set; }
        public string Colour { get; set; }
        public bool InProgress { get; set; } = false;
        public int WinCount { get; set; } = 0;
        public bool IsFilled { get; set; } = false;
        public bool IsJoined { get; set; } = false;
        public List<PlayerList> playerLists { get; set; }
    }
    public class PlayerList
    {
        public long? teamId { get; set; }
        public long? playerId { get; set; }
        public string playerName { get; set; }
        public string role { get; set; }
        public string sequence { get; set; } = string.Empty;

    }
}

