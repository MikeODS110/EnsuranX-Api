using Ensuranx.Application.Response.Event;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces;

public interface IEventTeamMemberStatResultRepository
{
    public Task<List<EventTeamMemberStatResult>> CreateRange(List<EventTeamMemberStatResult> eventTeamMemberStatResultList);
    public Task<List<EventTeamMemberStatResult>> UpdateRange(List<EventTeamMemberStatResult> eventTeamMemberStatResultList);

    public Task<List<EventTeamMemberStatResult>> GetTeamMembersPreviousDataAsync(List<long> teamMemberIdList);

    public Task<List<EventTeamMemberStatResult>> GetEventTeamMemberStatResultByEventIdAsync(long eventId);
    public Task<List<EventPlayedByPlayerCount>> GetEventTeamMemberStatResultByEventIdAsync(List<long> playerIdList);
}
