using Ensuranx.Domain.IdentityExtensions;

namespace Ensuranx.Application.Interfaces
{
    public interface IRoleRepository
    {
       // public Task<List<AppRole>> GetRolesList();
        public int GetRoleByUserId(int userId);
        public Task<string> GetRole(long userId, long roleId);
    }
}
