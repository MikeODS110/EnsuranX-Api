using Ensuranx.Application.Response.Team;

namespace Ensuranx.Application.Response.Tournament
{
    public class GetTournamentEvent
    {
        public string TournamentName { get; set; }
        public List<Event> EventDetailList { get; set; }
        public string TournamentFormat { get; set; } = string.Empty;
    }

    public class Event
    {
        public long EventId { get; set; }
        public string EventName { get; set; }
        public long EventCollectionId { get; set; }
        public List<TeamDetailWithStatus> TeamList { get; set; }
        public long VenueId { get; set; }
        public string VenueName { get; set; }
        public string EventDateTime { get; set; }
    }
}
