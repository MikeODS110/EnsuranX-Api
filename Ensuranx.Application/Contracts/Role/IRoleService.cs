using Ensuranx.Domain.IdentityExtensions;
using Microsoft.AspNetCore.Identity;

namespace Ensuranx.Application.Contracts.Role
{
    public interface IRoleService
    {
        public Task<List<AppRole>> GetRolesList();
    }
}
