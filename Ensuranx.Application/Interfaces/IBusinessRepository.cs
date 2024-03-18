using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IBusinessRepository
    {
        public Task<List<BusinessPackages>> GetBusinessPackagesAsync(long businessType);
        public Task<UserBusinessPackage> GetUserBusinessPackageByUserIdAsync(long userId);
        public Task<Business> GetBusinessByUserIdAsync(long userId);
        public Task<bool> CheckIfTheUserCompletedTheBusinessDetails(long userId);
    }
}
