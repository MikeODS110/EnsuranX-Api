using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class BusinessRepository: IBusinessRepository
    {
        private readonly ApplicationDbContext _context;

        public BusinessRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BusinessPackages>> GetBusinessPackagesAsync(long businessType)
        {
            return await _context.BusinessPackage.Include(x => x.BusinessPackageType).AsNoTracking().ToListAsync();
        }

        public async Task<UserBusinessPackage> GetUserBusinessPackageByUserIdAsync(long userId)
        {
            return await _context.UserBusinessPackage.FirstOrDefaultAsync(x => x.UserInfoId == userId);
        }

        public async Task<Business> GetBusinessByUserIdAsync(long userId)
        {
            return await _context.Business.FirstOrDefaultAsync(x => x.BusinessOwnerId == userId);
        }

        public async Task<bool> CheckIfTheUserCompletedTheBusinessDetails(long userId)
        {
            var checkBusiness = await _context.Business.AsNoTracking().AnyAsync(x => x.BusinessOwnerId == userId);
            var checkBusinessPackage = await _context.UserBusinessPackage.AsNoTracking().AnyAsync(x => x.UserInfoId == userId);
            var checkCreditCard = await _context.CreditCardInfo.AsNoTracking().AnyAsync(x => x.UserInfoId ==userId);

            return checkBusiness && checkBusinessPackage && checkCreditCard ? true : false;
        }
    }
}
