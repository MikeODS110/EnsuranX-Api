using Ensuranx.Application.Response.Event;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IEventTeamRepository
    {
        public Task<List<EventTeam>> GetEventTeamByEventIdAsync(long eventId);
        public Task<EventTeam> GetEventTeamByEventCollectionTeamIdAsync(long eventCollectionTeamId);
        public Task<EventTeam> GetEventTeamByEventCollectionIdAndTeamIdAsync(long eventCollectionId, long teamId);
        public Task<EventTeam> GetEventTeamByEventCollectionIdAndPlayerIdAsync(long eventCollectionId, long teamId, string eventStatus);
        public Task<List<TeamMemberMatchesWin>> GetEventTeamWinByTeamMemberIdList(List<long> teamMemberList);
        public Task<List<EventTeam>> UpdateEventTeamAsync(List<EventTeam> eventTeamList);

        public Task<MatchSummaryRespList> getMatchSummary(long eventId, long userId, long teamOneScore, long teamTwoScore, long statId, long teamMemberId);
        public Task<List<MatchSummaryRespList>> getMatchSummary(long eventId, long userId);

        public Task<DateTime> GetStartEventDateTime(long eventId);
        public Task<DateTime> GetEndEventDateTime(long eventId);
        public Task<List<long>> getEventlogIds(long eventId, DateTime startdate, DateTime endDate);

        public Task<List<EventLogResp>> getEventlog(BasicFilter paginationFilter, List<long> eventLogId);

        public Task<List<EventTeams>> getEventTeam(long eventId);
        public Task<EventTeam> GetActiveEventInEventCollection(long eventCollectionId);
    }
}
