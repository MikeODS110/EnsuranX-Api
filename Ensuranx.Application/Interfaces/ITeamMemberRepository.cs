using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Team;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface ITeamMemberRepository
    {
        public Task<List<TeamMember>> GetTeamMembersList(long userId, List<long> teamIdList);
        public Task<TeamMember> CheckIfThisPlayerIsPresentInWhichTeamList(long playerId, List<long> teamIdList);
        public Task<TeamMember> GetTeamMemberByTeamIdFirstOrDefault(long teamId);
        public Task<List<TeamMember>> GetTeamMembersListByTeamId(long teamId);
        public Task<List<TeamMember>> GetTeamMembersListWhichPlayedWithThisTeamIdLastMatchCount(long teamId, int lastMatchCount, List<long> totalTeamMemberIdList);
        public Task<TeamMember> GetCaptainOfTheTeam(long teamId);
        public Task<List<PlayerDetails>> GetAllPlayerAsyncByTeamId(long teamId, string sSearch);
        public int GetTeamMemberRequestAcceptedCount(long teamId);
        public int GetTeamMemberCountByTeamId(long teamId);
        public Task<List<TeamMemberMatchesWon>> GetTeamMembersMatchesWonList(long userId, List<long> teamMembersIdList);
        public Task<TeamMember> GetTeamMemberRequestAcceptedByTeamIdAndPlayerId(long teamId, long playerId);
        public Task<List<TeamMember>> GetTeamMemberRequestByTeamId(long teamId);
        public Task<List<TeamMember>> GetTeamMemberListRequestNotAcceptedByTeamId(long teamId);
        public Task<TeamMember> GetTeamMemberRequestNotAcceptedByTeamIdAndPlayerId(long teamId, long playerId);
        public Task<TeamMember> GetTeamMemberRequestAcceptedButDeletedByTeamIdAndPlayerId(long teamId, long playerId);
        public Task<TeamMember> CheckIfPlayerIsAMemberOfAnyTeamInEventCollectionById(long playerId, long eventCollectionId);
        public IQueryable<long> GetTeamsIdByPlayerIdList(long userId);
        public Task<TeamMember> checkCaption(long playerId);

        public Task<List<string>> getTeamJersey(long teamId,long userId);

        public Task<List<GetAllAvgResp>> GetAvgTournamentEvents(long userId, long tournamentId, long venueId);

        //public GetAllAvgResp GetAvgCal(List<GetAllAvgResp> getAllAvgResp);

        public Task<List<GetVenueEvent>> GetVenuesForAvg(long userId, long venueId);
        public Task<List<GetAllAvgResp>> GetAvgOnEvent(long userId, List<long> eventListId);
        public Task<List<TeamMember>> GetTeamMembersAllList(long userId, List<long> teamIdList);
        public Task<long> GetTeamMemberVsTournament(long tournamentId, long userId);
        public Task<TeamMember> GetTeamMemberByTeamIdNotSubstituteFirstOrDefault(long teamId);
        public List<long> GetEventVsTournament(long team, long userId);
        public Task<List<TeamMember>> GetTeamMembersDeletedList(long userId, List<long> teamIdList);

        public Task<List<long>> GetEventsaByEventType(long eventType, long userId);
        public Task<List<TeamMember>> DeleteTeamMemberList(List<TeamMember> teamMemberList);
        public Task<List<TeamMember>> GetTeamMemberListByEventId(long eventId);
        public Task<List<TeamMember>> GetTeamMembersAllPlayedList(long userId, List<long> teamIdList);
    }
}
