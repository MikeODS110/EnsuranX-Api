using Ensuranx.Application.Requests.Event;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IEventCollectionTeamRepository
    {
        public Task<List<EventCollectionTeam>> GetEventCollectionTeamList(CreateEvent createEvent);
        public Task<List<EventCollectionTeam>> CreateEventCollectionTeamList(List<EventCollectionTeam> eventCollectionList);
        public Task<string> GetMaxSequenceEventCollectionTeamByEventCollectionId(long eventCollectionId);
        public Task<List<long>> GetTeamIdListForAllTeamInTheSameTeamEventCollection(long teamId);
        public Task<long> GetEventCollectionIdByTeamId(long teamId);
        public Task<EventCollectionTeam> GetEventCollectionIdByTeamIdAndEventCollectionIdAsync(long teamId,long eventCollectionId);


    }
}
