using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using static Ensuranx.Domain.Common.Errors.Errors;

namespace Ensuranx.Infrastructure.Repositories
{
    public class TeamMemberRoleRepository : ITeamMemberRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamMemberRoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TeamMemberRole> GetTeamMemberRoleByName(string Name)
        {
            return await _context.TeamMemberRole.AsNoTracking().Where(x => x.Name.ToLower() == Name.ToLower()).FirstOrDefaultAsync();
        }
        public async Task<TeamMember> CheckMemberIsPresent(long teamId, long userId)
        {
            var results = await _context.TeamMember.AsNoTracking()
            .Where(tm => tm.TeamId == teamId && tm.PlayerId == userId).FirstOrDefaultAsync();

            return results;
        }
        public async Task<WaitingList> GetWaitingListPlayer(long userId, long eventCollectionId)
        {
            var results = await _context.WaitingList.FirstOrDefaultAsync(x => x.UserInfoId == userId && x.EventCollectionId == eventCollectionId);
            return results;
        }
    }
}
