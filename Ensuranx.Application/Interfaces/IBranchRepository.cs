using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IBranchRepository
    {
        public Task<List<BranchScheduling>> GetBranchSchedulingList(long branchId);
        public Task<List<BranchSchedulingDetailByBranchId>> GetBranchSchedulingListById(long branchId);
        public Task<List<Branch>> GetBranchByUserId(long userId);
        public Task<List<AllBranchesResponse>> GetAllBranchesList(long userId);
        public Task<List<GenericObj>> GetAllBranchesListByID(long userId);
        public Task<List<MatchesByIdRes>> GetMatchesByIdRepo(long eventCollectionId, long userId);

        public Task<List<StatkeeperImagesDetails>> GetStatkeeeperDetails(List<long> BranchId);

        public Task<long> getTotalVenueByBranch(long branchId);
        public Task<long> getTotalSportByBranch(long branchId);
        public Task<long> getTotalMatchByBranch(long branchId);

        public Task<Ensuranx.Domain.Entities.Branch> UpdateBranch(Ensuranx.Domain.Entities.Branch branch);
        //public Task<EventByUserId> GetAllEvents(long userId);

        public Task<BranchDetailByBranchId> getBranchDetail(long branchId);

        public Task<List<ConnectionDetailsbyBranchId>> connectionDetailsbyBranchIds(long branchId);
    }
}
