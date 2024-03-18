using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class UserInfoRepository : IUserInfoRepository
    {
        private readonly ApplicationDbContext _context;

        public UserInfoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public UserInfo CheckForValidOtpCode(int otpCode, string email)
        {
            return _context.UserInfo.Include(x => x.User).Where(x => x.User.Email == email && x.OTPCode == otpCode && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        }

        public UserInfo GetUserInfoByUserId(long userId)
        {
            return _context.UserInfo.Where(x => x.Id == userId && x.IsActive == true && x.IsDeleted == false).Include(x => x.User).FirstOrDefault();
        }
        public UserInfo GetUserInfoByUserTableId(long userId)
        {
            return _context.UserInfo.Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false).Include(x => x.User).FirstOrDefault();
        }

        public IQueryable<UserInfo> GetUserInfoByUserEmailList(IQueryable<string> tournamentEmailList)
        {
            return _context.UserInfo.AsNoTracking().Where(x => tournamentEmailList.Contains(x.User.Email)).AsQueryable();
        }

        public UserInfo GetUserInfoByUserEmail(string email)
        {
            return _context.UserInfo.AsNoTracking().Include(x => x.User)
                .Where(x => x.User.Email == email && x.IsEmailVerified != false && x.IsActive != false && x.IsDeleted != true).FirstOrDefault();
        }

        public UserInfo GetUserInfoByUserIdAsync(int userId)
        {
            return _context.UserInfo.Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        }

        public UserInfo GetUserInfoByUserInfoId(long userInfoId)
        {
            return _context.UserInfo.Where(x => x.Id == userInfoId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        }

        public UserInfo UpdateUserInfo(UserInfo userInfo)
        {
            _context.UserInfo.Update(userInfo);
            _context.SaveChanges();
            return userInfo;
        }

        public async Task<List<UserInfo>> GetUserInfoByUserId(List<long> userIdList)
        {
            return await _context.UserInfo.AsNoTracking().Where(x => userIdList.Contains(x.Id) && x.IsActive == true && x.IsDeleted == false).ToListAsync();
        }
     

     
        public Ensuranx.Domain.Entities.UserInfo GetOpt(int otp)
        {
            var userInfo = new Ensuranx.Domain.Entities.UserInfo();
            userInfo = _context.UserInfo.Where(x => x.OTPCode == otp).Include(x => x.User).FirstOrDefault();

            return userInfo;
        }
    }
}
