using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Team;
using Ensuranx.Common.PaginationResponse;

namespace Ensuranx.Application.Interfaces
{
    public interface IBranchesVenuesRepository
    {
        public Task<List<BranchesVenuesResponseTemp>> GetBranchesVenuesListByBusiness(BasicFilter paginationFilter, long userId, long teamId, long tournamentId, long venueId, Domain.Enums.Enum.EventStatus eventStatus);
        public Task<int> GetBranchesVenuesCount(long userId, long teamId, long tournamentId, long venueId, Domain.Enums.Enum.EventStatus eventStatus, string search);
        public Task<List<GetAllTeamsByVenueId>> GetAllTeamsByVenueId(List<long> venueId);
        public Task<List<BranchesVenuesResponseTemp>> GetBranchesVenuesListByStatkeeper(BasicFilter paginationFilter, long tournamentId, long userId, long teamId, long venueId, Domain.Enums.Enum.EventStatus evnetStatus);
        public Task<int> GetBranchesVenuesListByStatkeeperCount(long userId, long teamId, long tournamentId, long venueId, Domain.Enums.Enum.EventStatus evnetStatus, string search);
        public Task<List<BranchesVenuesResponse>> GetBranchesVenuesListByPlayer(BasicFilter paginationFilter, long userId, long tournamentId, long teamId, long venueId,
            Domain.Enums.Enum.EventStatus evnetStatus, long activityCategoryId);
        public Task<int> GetBranchesVenuesPlayerCount(BasicFilter paginationFilter, long userId, long tournamentId, long teamId, long venueId, Domain.Enums.Enum.EventStatus evnetStatus,
            long activityCategoryId, string sSearch);
        public Task<List<TeamDetail>> GetBranchesVenuesListByTeam(List<long> eventsId);
        public Task<List<Icon>> GetBranchesVenuesListActivityIcons(List<long> ActivityId);
        public Task<List<Icon>> GetBranchesVenuesListStatkeeperIcons(List<long> statkeeperId);

        public Task<string> GetStatusByUserId(long userId, List<TeamDetail> Teams);
        public Task<List<BranchesVenuesResponse>> GetBranchesVenuesListByPlayerTemp(BasicFilter paginationFilter, long userId, long teamId, long tournamentId,
           long venueId, Domain.Enums.Enum.EventStatus eventStatus, long activityCategoryId);



    }
}
