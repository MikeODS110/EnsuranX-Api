using ErrorOr;
using Ensuranx.Application.Requests.Branch;
using Ensuranx.Application.Requests.Venue;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.User;
using Ensuranx.Application.Response.Venue;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Contracts.Venue
{
    public interface IVenueService
    {
        public Task<ErrorOr<List<VenueResponse>>> GetAllVenuesAsync(long userId, BranchIdList branchIdList, long roleId);
        public Task<ErrorOr<AddVenueResponse>> AddVenueAsync(long userId, string email, AddVenue addVenue);

        public Task<ErrorOr<string>> UpdateEventVenueList(UpdateEventVenueListReq VenueEventListUpdateReq);
        public Task<ErrorOr<string>> UpdateVenueStatkeeper(UpdateVenueStatkeeperReq UpdateVenueStatkeeperReq);

        public Task<ErrorOr<BranchesVenuesListById>> BranchesVenuesListById(long EventId);

        public Task<ErrorOr<DeleteEventCollection>> DeleteEventCollectionById(long eventCollectionId);

        public Task<ErrorOr<List<VenueByBrandIdResp>>> GetVenuesByBranchId(long branchId);
        public Task<ErrorOr<GenericMessage>> UpdateVenue(UpdateVenueReq UpdateVenueReq,string email);
    }
}
