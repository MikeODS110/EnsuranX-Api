using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class BranchSchedulingRepository : IBranchSchedulingRepository
    {
        private readonly ApplicationDbContext _context;

        public BranchSchedulingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchScheduling>> CreateBranchScheduling(List<BranchScheduling> branchSchedulings)
        {
            await _context.BranchScheduling.AddRangeAsync(branchSchedulings);
            await _context.SaveChangesAsync();
            return branchSchedulings;
        }

        public async Task<List<BranchScheduling>> GetBranchSchedulingByBranchIdList(List<long> branchIdList)
        {
            return await _context.BranchScheduling.AsNoTracking().Where(x => branchIdList.Contains(x.BranchId)).ToListAsync();
        }
    }
}
