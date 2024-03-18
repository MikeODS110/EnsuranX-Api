using ErrorOr;
using Ensuranx.Application.Requests.Branch;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;

namespace Ensuranx.Application.Contracts.Branches
{
    public interface IBranchesService
    {
        // public Task<ErrorOr<PagedResponse<List<BranchesVenuesResponse>>>> GetAllBranchesAndVenuesAsync(PaginationFilter paginationFilter, int userId);
        public Task<ErrorOr<PagedResponse<List<BranchesVenuesResponse>>>> GetAllBranchesVenuesAsync(BasicFilter paginationFilter, long userId, long teamId, long roleId, long tournamentId, long venueId,
            Domain.Enums.Enum.EventStatus eventStatus, long activityCategoryId);
        public Task<ErrorOr<List<AllBranchesResponse>>> GetAllBranchesAsync(long userId, long teamId);
        public Task<ErrorOr<List<GenericObj>>> GetAllBranchesAsync(long userId);
        public Task<ErrorOr<UpdateBranchResponse>> UpdateBranchesAsync(UpdateBranch updateBranch, string email);
        public Task<ErrorOr<AddBranchResponse>> AddBranchAsync(long userId, string email, AddBranch addBranch);
        public Task<ErrorOr<List<MatchesByIdRes>>> GetMatchesById(long eventCollectionId, long userId);
        public Task<ErrorOr<DeleteStatkeeperById>> DeleteBranchByIdAsync(long BranchId);

        public Task<ErrorOr<BranchDetailByBranchId>> GetBranchDetailByBranchIdAsyn(long branchId);
    }
}
