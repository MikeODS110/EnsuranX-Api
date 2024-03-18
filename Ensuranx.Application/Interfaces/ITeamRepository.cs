using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Team;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.PaginationResponse;
using Microsoft.Identity.Client;
using Ensuranx.Domain.Entities;
using Microsoft.Extensions.Logging;
using static Ensuranx.Domain.Common.Errors.Errors;

namespace Ensuranx.Application.Interfaces
{
    public interface ITeamRepository
    {
        public Task<List<TeamMemberDetails>> GetTeamsWithMembersListByPlayerId(string role, long userId, BasicFilter basicFilter);
        public Task<List<GetRecentTeam>> GetRecentTeamsByPlayerIdAsync(long userId, BasicFilter basicFilter);
        public Task<int> GetRecentTeamsCountByPlayerIdAsync(long userId);
        public Task<List<TeamMemberDetails>> GetTeamsWithMembersListByBusinessId(long userId, BasicFilter basicFilter);
        public Task<List<GetAllInvites>> GetAllInvitesToJoinTeamList(long teamId);
        public Task<List<TeamGenericObj>> GetTeamForDropdownAsync(long userId, Domain.Enums.Enum.TeamType teamType, long tournamentId);
        public Task<Domain.Entities.Team> GetTeamById(long teamId);
        public Task<Domain.Entities.Team> DeleteTeamById(Domain.Entities.Team team);
        public Task<DateTime> GetLastPlayedDateTimeByTeamId(long teamId);
        public Task<int> GetMatchesWonByTeamId(long teamId, int lastDays);
        public Task<int> GetMatchesTieByTeamId(long teamId, int lastDays);
        public Task<int> GetTeamCountInEventCollection(long eventCollectionId);

        public Task<int> GetMatchesLoseByTeamId(long teamId, int lastDays);
        public Task<GetTeamDetails> GetTeamsDetailByTeamId(long userId, long teamId);
        public Task<List<long>> GetTeamsIdListByPlayerId(long userId);
        public Task<List<MemberDetail>> GetPlayersDetailsByTeamId(long userId, long teamId,string sSearch,bool isCaptain);
        public Task<List<EventTeamMemberStatResult>> GetPlayersStatsByTeamId(long userId, long teamId);
        public Task<List<GetEventDetails>> GetTeamMatchesByTeamId(long teamId, int lastDays);
        public Task<List<GetAllTournament>> GetTeamTournamentByTeamId(long teamId, int lastDays);
        public Task<int> GetTeamsWithMembersCount(long userId);
        public Task<int> GetTeamsWithMembersCountPlayer(long userId, string sSearch);
        public Task<bool> CheckTeamNameTaken(string name);

        public Task<string> GetRole(long roleId, long userId);
        public Task<List<GetTeamDetailsByEventCollectionRes>> GetTeamsByEventCollection(long eventCollectionId, long userId);
        public Task<List<GetTeamDetailsByEventCollectionRes>> GetTeamsByEventId(long eventId, long userId);
        //public List<long> GetAllEvents(long userId);
        public List<long> GetAllEvents(long userId, Domain.Enums.Enum.EventStatus eventStatus);

        /*public IQueryable<TeamDetail> GetAllteamsOnEvent(IQueryable<long> eventId);*/
        public List<TeamDetail> GetAllteamsOnEvent(List<long> eventId);
        // public Task<string> searchedTeam(string search, List<GenericObj> teamsOnEvent);
        
        public Task<List<PlayerList>> GetPlayersbyTeamId(List<long> teamId);


        public Task<List<GenericObj>> GetTeamsByEventCollectionId(long eventCollectionId, long userId);

    }
}
