using Ensuranx.Application.Contracts.Role;
using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Ensuranx.Infrastructure.Services.Role
{
    public class RoleService : IRoleService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly IRoleRepository _iroleRepository;

        public RoleService(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _iroleRepository = new RoleRepository(context);
        }

        public async Task<List<AppRole>> GetRolesList()
        {
            //return await _iroleRepository.GetRolesList();
            return new List<AppRole> { new AppRole { Id = 1, } }
;        }
    }
}
