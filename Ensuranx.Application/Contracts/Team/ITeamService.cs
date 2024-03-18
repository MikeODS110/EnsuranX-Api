using ErrorOr;
using Ensuranx.Application.Requests.Team;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Team;
using Ensuranx.Common.PaginationResponse;

namespace Ensuranx.Application.Contracts.Team
{
    public interface ITeamService
    {
        public Task<ErrorOr<CreateTemporaryTeamResponse>> CreateTemporaryTeamAsync(long userId, string email,long eventCollectionId,long playerId);
        public Task<ErrorOr<GenericMessage>> AssignCaptainAsync(long userId, string email, AssignCaptainToTeam assignCaptainToTeam);
        public Task<ErrorOr<GenericMessage>> DeleteTeamAsync(long userId, string email, long teamId = 0);
        public Task<ErrorOr<CreateTeamResponse>> CreateOfficialTeamAsync(long userId, string email, CreateTeam createTeam);
        public Task<ErrorOr<GenericMessage>> UpdateTeamAsync(long userId, string email, UpdateTeam updateTeam);
        public Task<ErrorOr<RemoveOrBanPlayerResponse>> RemoveOrBanPlayerAsync(long userId, string email, RemoveOrBanPlayer removeOrBanPlayer);
        public Task<ErrorOr<GenericMessage>> InvitePlayerToJoinTeamAsync(long userId, string email, InvitePlayerToTeam invitePlayerToTeam);
        public Task<ErrorOr<GenericMessage>> InviteActionPlayerToJoinTeamAsync(long userId, string email, InviteActionToJoinTeam inviteActionToJoinTeam);
        public Task<ErrorOr<AddPlayerToTeamResponse>> AddPlayerToTheTeamAsync(long userId, string email,long teamId,long playerId,long eventCollectionId);
        public Task<ErrorOr<PagedResponse<List<TeamMemberDetails>>>> GetAllTeams(long roleId, long userId, BasicFilter basicFilter);
        public Task<ErrorOr<PagedResponse<List<GetRecentTeam>>>> GetRecentTeamsAsync(long userId, BasicFilter basicFilter, string email);
        public Task<ErrorOr<List<GetAllInvites>>> GetAllInvitesToJoinTeamAsync(long teamId);
        public Task<ErrorOr<List<TeamGenericObj>>> GetTeamForDropdownAsync(long userId,Domain.Enums.Enum.TeamType teamType, long tournamentId);
        public Task<ErrorOr<List<PlayerDetails>>> GetAllPlayerAsync(long userId,long teamId, string sSearch);
        public Task<ErrorOr<GetAllMembersListInvite>> GetInviteMembersDetailsAsync(long userId,long teamId,string sSearch);
        public Task<ErrorOr<GetTeamDetails>> GetTeamDetailsByTeamIdAsync(long teamId,long userId,int lastDays);
        public Task<ErrorOr<GetTeamPlayersStat>> GetPlayersStatsByTeamIdAsync(long teamId,long userId);
        public Task<ErrorOr<List<MemberDetail>>> GetPlayersDetailsByTeamIdAsync(long teamId, long userId,string sSearch, bool isCaptain);
        public Task<ErrorOr<List<GetTeamDetailsByEventCollectionRes>>> GetTemporaryTeamsByEventCollection(long userId, string email, long eventCollectionId,long eventId, long roleId);

        public Task<ErrorOr<GenericMessage>> BackToLobby(long playerId, long teamId, long eventCollectionId, string email);

        public Task<ErrorOr<GetAllAvgResp>> GetAllAvg(long userId, long tornamentId, long VenueId, long eventType);

    }
}
