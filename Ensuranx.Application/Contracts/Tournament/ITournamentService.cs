using ErrorOr;
using Ensuranx.Application.Requests.Tournament;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Team;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Application.Contracts.Tournament
{
    public interface ITournamentService
    {
        public Task<ErrorOr<GenericMessage>> CreateTournamentAsync(CreateTournament createTournament, string email, long userId);
        public Task<ErrorOr<GetTournament>> GetTournamentByIdAsync(long id, string email, long userId);
        public Task<ErrorOr<PagedResponse<List<GetAllTournament>>>> GetAllTournamentAsync(string email, long userId, BasicFilter paginationFilter, TournamentStatus tournamentStatus, 
            long teamId, long roleId,long venueId, long activityCategoryId);
        public Task<ErrorOr<RequestToJoinResponse>> RequestToJoinAsync(string email, long userId, long tournamentId, long teamId);
        public Task<ErrorOr<string>> RandomizeTeamAsync(string email, long userId, long tournamentId);
        public Task<ErrorOr<string>> AddStatkeeperAsync(string email, long userId, long tournamentId, int statkeeperId);
        public Task<ErrorOr<List<AllStatkeepersVenueList>>> GetStatkeeperByIdAsync(string email, long userId, long tournamentId);
        public Task<ErrorOr<List<GenericObj>>> GetStatkeeperListByTournamentAsync(string email, long userId, long tournamentId);
        public Task<ErrorOr<List<TeamGenericObj>>> GetTournamentTeamsAsync(string email, long userId, long tournamentId,long roleId);
        public Task<ErrorOr<PagedResponse<List<RequestedTeamsInTournament>>>> GetTournamentTeamsRequestAsync(BasicFilter paginationFilter, string email, long userId, long tournamentId);
        public Task<ErrorOr<GetTournamentEvent>> GetTournamentEventsByIdAsync(string email, long userId, long tournamentId);
        public Task<ErrorOr<List<GetTournamentResult>>> GetTournamentResultByIdAsync(string email, long userId, long tournamentId);
        public Task<ErrorOr<List<PlayoffGroups>>> GetTournamentPlayoffByIdAsync(string email, long userId, long tournamentId);
        public Task<ErrorOr<string>> DeleteTournamentStatkeeperAsync(string email, long userId, long tournamentId, int statkeeperId);
        public Task<ErrorOr<AcceptRequestToJoinResponse>> AcceptRequestToJoinAsync(string email, long userId, long tournamentId, long teamId, bool accept);
        public Task<Domain.Entities.Tournament> CreateAsync(Domain.Entities.Tournament tournament);

        public Task<ErrorOr<List<TournamentPrizeRes>>> GetTournamentPrizes(long tournamentId);
        public Task<ErrorOr<string>> SchedulePlayoffMatchesAsync(string email, long userId, long tournamentId);
       
        public Task<ErrorOr<string>> updatePrizeReq(UpdatePrizeReq updatePrizeReq);
        public Task<ErrorOr<GenericMessage>> UpdateTournament(UpdateTournamentReq updateTournament, string email);
        public Task<ErrorOr<List<GenericObj>>> GetAllTournamentDropdownAsync(string email, long userId,long activityCategoryId);
        
    }
}
