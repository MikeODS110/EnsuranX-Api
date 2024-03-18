

using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task<List<AppRole>> GetRolesList()
        //{
        //    var admin = Domain.Enums.Enum.Roles.Admin.ToString().ToLower();
        //    //var statkeeper = Domain.Enums.Enum.Roles.Statkeeper.ToString().ToLower();

        //    //var result = await _context.Roles.AsNoTracking().Where(x => x.Name.ToLower() != admin && x.Name.ToLower() != statkeeper).ToListAsync();

        //    //return result;
        //}

        public int GetRoleByUserId(int userId)
        {
            var results = _context.UserRoles.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.RoleId).FirstOrDefault();
            return results;
        }

        public async Task<string> GetRole(long userId, long roleId)
        {
            var results = await (from userRole in _context.UserRoles
                                 join user in _context.Users
                                 on userRole.UserId equals user.Id

                                 join userInfo in _context.UserInfo
                                 on user.Id equals userInfo.UserId

                                 join role in _context.Roles
                                 on userRole.RoleId equals role.Id

                                 where role.Id == roleId

                                 select role.Name).FirstOrDefaultAsync();


            return results;
        }
    }
}
