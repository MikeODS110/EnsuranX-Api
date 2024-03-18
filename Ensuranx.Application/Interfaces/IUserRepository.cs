using Ensuranx.Domain.IdentityExtensions;

namespace Ensuranx.Application.Interfaces
{
    public interface IUserRepository
    {
        public Task<List<AppUser>> GetUserByUserIdList(List<long> userIdList);
        public Task<AppUser> GetUserByUserId(long userId);
        public Task<bool> CheckPhoneNumber(string number);
        
    }
}
