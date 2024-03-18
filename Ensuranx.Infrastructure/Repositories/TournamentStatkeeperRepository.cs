using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class TournamentStatkeeperRepository : ITournamentStatkeeperRepository
    {
        private readonly ApplicationDbContext _context;

        public TournamentStatkeeperRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AllStatkeepersVenueList>> GetStatkeeprList(long tournamentId)
        {
            var query = await (from tournamentStatkeeper in _context.TournamentStatkeeper
                               join tournament in _context.Tournament.Where(x => x.Id == tournamentId)
                               on tournamentStatkeeper.TournamentId equals tournament.Id

                               join venueStatkeeper in _context.VenueStatkeeper.Include(x => x.Venue.Branch)
                               on tournamentStatkeeper.StatkeeperId equals venueStatkeeper.UserInfoId

                               join userInfoTbl in _context.UserInfo.Include(x => x.User)
                               on venueStatkeeper.UserInfoId equals userInfoTbl.Id

                               group venueStatkeeper by venueStatkeeper.UserInfoId into venueStatkep

                               select new AllStatkeepersVenueList
                               {
                                   UserId = venueStatkep.Key.Value,
                                   Name = venueStatkep.First().UserInfo.Name,
                                   BranchName = venueStatkep.First().Venue.Branch.Name,
                                   VenueList = venueStatkep.Select(t => new GenericObj
                                   {
                                       Name = t.Venue.Name,
                                      Id = t.Venue.Id,
                                   }).ToList(),
                                   PhoneNumber = venueStatkep.First().UserInfo.User.PhoneNumber,
                                   Email = venueStatkep.First().UserInfo.User.Email
                               }).ToListAsync();

            return query;
        }
        
        public async Task<TournamentStatkeeper> GetTournamentStatkeeperByIdsAsync(long tournamentId,int statkeeperId)
        {
            return await _context.TournamentStatkeeper.AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted && x.TournamentId == tournamentId && x.StatkeeperId == statkeeperId)
                .FirstOrDefaultAsync();
        }
    }
}
