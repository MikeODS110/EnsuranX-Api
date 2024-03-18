using Ensuranx.Application.Response.Event;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IEventLogRepository
    {
        public Task<EventLog> GetEventLogByEventMemberStatId(long eventId, long teamMemberId,long statId, bool isSuccess);
        public Task<List<EventLog>> GetEventLogListByEventId(long eventId, long userId);

        public Task<TeamScorePoints> getTeamScoreByPlayer(long eventId);
    }
}
