using Ensuranx.Application.Requests.Venue;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Application.Response.Venue;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;


namespace Ensuranx.Application.Interfaces
{
    public interface IVenueRepository
    {
        public Task<List<VenueResponse>> GetAllVenuesAsyncByStatkeeper(long userId, List<long> branchId);
        public Task<int> GetAllVenuesCountByStatKeeper(long userId, List<long> branchId);
        public Task<List<VenueResponse>> GetAllVenuesAsyncByBusiness(long userId, List<long> branchId,long statkeeperId, long roleId);
        public Task<int> GetAllVenuesCountByBusiness(long userId, List<long> branchId);

        public Task<List<Venue>> GetVenuesByIdListAsync(List<long> venueIdList);
        public Task<List<GetVenueBrnach>> GetVenuesByIdUserIdAsync(long userId);
        public Task<List<GenericObj>> GetBranchesByIdUserIdAsync(long userId);

        public Task<List<GetVenuesEventWithoutTeams>> GetVenuesEventWithoutTeamsBusiness(BasicFilter paginationFilter, long userId);
        public Task<int> GetVenuesEventWithoutTeamsBusinessCount(BasicFilter paginationFilter, long userId);
        public Task<List<GetVenuesEventWithoutTeams>> GetVenuesEventWithoutTeamsPlayer(BasicFilter paginationFilter, long userId);
        public Task<int> GetVenuesEventWithoutTeamsPlayerCount(long userId, string sSearch);
        public Task<List<GetVenuesEventWithoutTeams>> GetVenuesEventWithoutTeamsStatkeeper(BasicFilter paginationFilter, long userId);
        // public Task<List<VenueEventListResponse>> GetVenuesEventWithoutTeamsStatkeeper(BasicFilter paginationFilter, long userId);
        public Task<int> GetVenuesEventWithoutTeamsStatkeeperCount(long userId, string search);
        public Task<List<GetVenuesEventWithoutTeams>> StatkeeperInVenue(BasicFilter paginationFilter, long userId);
        public Task<List<GetAllTeamsByVenueId>> GetAllTeamsByVenueId(List<long> venueId);
        public Task<int> GetVenuesEventCount(long userId);
        public Task<BranchesVenuesListById> GetBranchesVenuesListById(long EventId);

        public Task<BranchesVenuesListById> UpdateEventList(UpdateEventVenueListReq VenueEventListUpdateReq);
        public Task<long> EventStatusId(Domain.Entities.Event Event);
        public Task<long> EventTeamId(Domain.Entities.Event Event);

        public Task<List<GetAllPlayersByEC>> GetAllPlayers(List<long> eventCollectionId);
        public Task<List<GetAllPlayersByEC>> GetAllPlayersByPlayer(List<long> eventId);

        public Task<List<VenueByBrandIdResp>> GetVenuesDetail(long BranchId);

        public Task<List<StatkeeperImagesDetails>> GetStatkeeperDetails(List<long> venueId);

        public Task<List<currentActivityResp>> GetCurrentActivity(List<long> venueId);


    }
}
