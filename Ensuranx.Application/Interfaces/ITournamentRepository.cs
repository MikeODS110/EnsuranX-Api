using ErrorOr;
using Ensuranx.Application.Requests.Tournament;
using Ensuranx.Application.Response.Team;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Application.Interfaces
{
    public interface ITournamentRepository
    {

        public IQueryable<Domain.Entities.Tournament> SearchInTournament(string sSearch);
        public IQueryable<Domain.Entities.Tournament> GetAll(long activityCategoryId);
        public Task<Tournament> GetTournamentByIdAsync(long id, long userId);
        public Task<List<TournamentPrizeRes>> TournamentPrizeQuery(long tournamentId);
        public Task<List<GetAllTournament>> GetAllTournamentAsyncByBusiness(long userId, BasicFilter paginationFilter, TournamentStatus tournamentStatus, long teamId, long venueId);
        public Task<int> GetAllTournamentCountAsyncByBusiness(long userId, TournamentStatus tournamentStatus, long teamId, long venueId);
        public Task<List<GetAllTournament>> GetAllTournamentAsyncByPlayer(long userId, BasicFilter paginationFilter, TournamentStatus tournamentStatus, long teamId, long venueId,
                                                                         IQueryable<Domain.Entities.Tournament> tournamentTbl, IQueryable<TournamentTeam> tournamentTeamList, IQueryable<TournamentVenue> tournamentVenueList,
                                                                         IQueryable<Images> imageActivityList, IQueryable<UserInfo> userInfo, IQueryable<long> teamIdList, IQueryable<Images> imageCreatedByImageList);
        public Task<int> GetAllTournamentCountAsyncByPlayer(long userId, TournamentStatus tournamentStatus, long teamId, long venueId,
                                                                         IQueryable<Domain.Entities.Tournament> tournamentTbl, IQueryable<TournamentTeam> tournamentTeamList, IQueryable<TournamentVenue> tournamentVenueList,
                                                                         IQueryable<Images> imageActivityList, IQueryable<UserInfo> userInfo, IQueryable<long> teamIdList, IQueryable<Images> imageCreatedByImageList, string sSearch);
        public Task<List<GetAllTournament>> GetAllTournamentAsyncByStatkeeper(long userId, BasicFilter paginationFilter, TournamentStatus tournamentStatus, long teamId, long venueId);
        public Task<int> GetAllTournamentCountAsyncByStatkeeper(long userId, TournamentStatus tournamentStatus, long teamId, long venueId);
        public Task<List<GenericObj>> GetStatkeeperListByTournamentIdAsync(long tournamentId);
        public Task<List<TeamGenericObj>> GetTournamentTeamsListAsync(long tournamentId, long userId);
        public Task<List<RequestedTeamsInTournament>> GetTournamentTeamsRequestAsync(BasicFilter paginationFilter, long tournamentId);
        public Task<int> GetTournamentTeamsRequestAsyncCount(long tournamentId);
        public Task<List<TournamentPlayoffResponse>> GetTournamentPlayoffByIdAsync(long tournamentId);
        public Task<List<Response.Tournament.Event>> GetTournamentEventsByIdAsync(long tournamentId);
        public Task<TournamentTeam> GetTournamentTeamByIdsAsync(long tournamentId, long teamId);
        public Task<bool> CheckTournamentActivityMatchesTeamActivityAsync(long tournamentId, long teamId);
        public Task<GetTournament> GetTournamentByIdJoinAsync(long id);
        public Task<string> CheckForTournamentIsJoinedByUserAnyTeam(long tournamentId, long userId);

        public Task<List<long>> GetTournamentTopTeamsToSchedulePlayoffRound(long tournamentId);
        public Task<bool> CheckForTournamentIsCaptainByUserAnyTeam(long tournamentId, long userId);
        public Task<bool> GetTournamentFormatByTournamentId(long tournamentId, long userId);

        public List<TournamentStatkeeper> getTournamentstatkeeper(UpdateTournamentReq updateTournament);
        public List<TournamentVenue> getTournamentVenue(UpdateTournamentReq updateTournament);

        public long getActivityId(long activityCatId);



    }
}
