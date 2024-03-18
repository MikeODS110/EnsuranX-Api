using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Statkeeper;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Repositories
{
    public class StatkeeperRepository : IStatkeeperRepository
    {
        private readonly ApplicationDbContext _context;

        public StatkeeperRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserInfo>> GetAllStatkeepersList(long venueId)
        {
            if (venueId != 0)
            {
                return await (from role in _context.Roles.Where(x => x.Name == Roles.Statkeeper.ToString())
                              join userrole in _context.UserRoles
                              on role.Id equals userrole.RoleId
                              join UserInfo in _context.UserInfo
                              on userrole.UserId equals UserInfo.UserId
                              join venuestatskeeper in _context.VenueStatkeeper.Where(x => x.VenueId == venueId)
                              on UserInfo.Id equals venuestatskeeper.UserInfoId
                              select new UserInfo { Name = UserInfo.Name, Id = UserInfo.Id }
                          ).ToListAsync();
            }
            else
            {
                return await (from role in _context.Roles.Where(x => x.Name == Roles.Statkeeper.ToString())
                              join userrole in _context.UserRoles
                              on role.Id equals userrole.RoleId
                              join UserInfo in _context.UserInfo
                              on userrole.UserId equals UserInfo.UserId
                              select new UserInfo { Name = UserInfo.Name, Id = UserInfo.Id }
                          ).ToListAsync();
            }
        }


        public async Task<List<VenuesStatskeeper>> GetAllStatkeepersByUserIdList(long userId, BasicFilter paginationFilter)
        {
            return await (from branch in _context.Branch.Where(x => x.UserInfoId == userId)
                          join venue in _context.Venue
                          on branch.Id equals venue.BranchId
                          join venuestatkeeper in _context.VenueStatkeeper
                          on venue.Id equals venuestatkeeper.VenueId

                          select venuestatkeeper
                         ).ToListAsync();
        }

        public Task<SubstitutePlayer> SubstitutePlayer(long teamId, long eventId, long substituteId, long substituteToId)
        {
            var results = (from teamMember in _context.TeamMember.AsNoTracking().Where(x => x.TeamId == teamId)

                           select new SubstitutePlayer
                           {
                               EventId = eventId,

                           });
            return (Task<SubstitutePlayer>)results;
        }
        public Task<bool> IsMember(long teamId, long TeamMember)
        {
            bool isMember = _context.TeamMember.Any(t => t.TeamId == teamId && t.PlayerId == TeamMember);
            return Task.FromResult(isMember);
        }
        public Task<Application.Response.User.SubstitutePlayer> Check(long teamId, long eventId, long substituteId, long substituteToId)
        {
            var results = from events in _context.Event.AsNoTracking().Where(x => x.Id == eventId)
                          join eventTeam in _context.EventTeam
                          on events.Id equals eventTeam.EventId

                          join eventCollectionTeam in _context.EventCollectionTeam.Where(x => x.TeamId == teamId)
                          on eventTeam.EventCollectionTeamId equals eventCollectionTeam.Id

                          join team in _context.Team
                          on eventCollectionTeam.TeamId equals team.Id

                          join teamMember in _context.TeamMember
                          .Where(x => x.PlayerId == substituteId && x.PlayerId == substituteToId)
                          on team.Id equals teamMember.TeamId
                          select new Application.Response.User.SubstitutePlayer
                          {
                              eventId = eventId,
                              teamId = teamId,
                              substituteId = substituteId,
                              substituteTo = substituteToId
                          };
            return (Task<Application.Response.User.SubstitutePlayer>)results;

        }

        public Task<List<GenericObj>> getVenuesByStatkeeperId(long statkeeperId)
        {
            var results = _context.VenueStatkeeper.Where(x => x.UserInfoId == statkeeperId && x.IsDeleted!=true && x.IsActive!=false)
                          .Select(x => new GenericObj
                          {
                              Id = x.Venue.Id,
                              Name = x.Venue.Name
                          }).Distinct().ToListAsync();
            return results;
        }
        public async Task<UpdateStakeeper> GetStatkeeperBio(long statkeeprId)
        {
            var results = await _context.UserInfo.Where(x => x.Id == statkeeprId).Include(x => x.User)
                            .Select(x => new UpdateStakeeper
                            {
                                StatkeeperId = x.Id,
                                FullName = x.Name,
                                Email = x.User.Email,
                                Password = x.User.PasswordHash,
                                PhoneNumber = x.User.PhoneNumber
                            }).FirstOrDefaultAsync();
            return results;
        }
        public async Task<UserInfo> GetUserTbl(long statkeeperId)
        {
            return await _context.UserInfo
                .Include(u => u.User) // Include the "User" navigation property
                .FirstOrDefaultAsync(u => u.Id == statkeeperId);
        }
    }
}

