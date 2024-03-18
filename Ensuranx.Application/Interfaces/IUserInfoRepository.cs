using ErrorOr;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IUserInfoRepository
    {
        public UserInfo CheckForValidOtpCode(int otpCode, string email);
        public UserInfo GetUserInfoByUserId(long userId);
        public UserInfo GetUserInfoByUserEmail(string email);
        public Ensuranx.Domain.Entities.UserInfo GetOpt(int opt);

        public UserInfo GetUserInfoByUserIdAsync(int userId);
        public UserInfo GetUserInfoByUserInfoId(long userInfoId);
        public UserInfo UpdateUserInfo(UserInfo userInfo);
        public UserInfo GetUserInfoByUserTableId(long userId);
        public Task<List<UserInfo>> GetUserInfoByUserId(List<long> userInfoIdList);
        public IQueryable<UserInfo> GetUserInfoByUserEmailList(IQueryable<string> tournamentEmailList);
       
    }
}
