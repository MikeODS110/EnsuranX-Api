using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppUser>> GetUserByUserIdList(List<long> userIdList)
        {
            return await _context.UserInfo.AsNoTracking().Where(x => userIdList.Contains(x.Id)).Select(x => x.User).ToListAsync();
        }

        public async Task<AppUser> GetUserByUserId(long userId)
        {
            return await _context.UserInfo.AsNoTracking().Where(x => x.Id == userId).Select(x => x.User).FirstOrDefaultAsync();
        }

        public async Task<bool> CheckPhoneNumber(string number)
        {
            return await _context.Users.Where(x => x.PhoneNumber == number).FirstOrDefaultAsync() != null ? true : false;
        }
    }
}
