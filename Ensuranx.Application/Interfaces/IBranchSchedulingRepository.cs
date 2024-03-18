using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IBranchSchedulingRepository
    {
        public Task<List<BranchScheduling>> CreateBranchScheduling(List<BranchScheduling> branchSchedulings);
        public Task<List<BranchScheduling>> GetBranchSchedulingByBranchIdList(List<long> branchIdList);
    }
}
