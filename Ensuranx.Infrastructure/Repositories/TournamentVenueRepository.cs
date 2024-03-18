using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class TournamentVenueRepository : ITournamentVenueRepository
    {
        private readonly ApplicationDbContext _context;

        public TournamentVenueRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TournamentVenue>> GetAllTournamentVenueAsync(long tournamentId)
        {
            return await _context.TournamentVenue
                                 .AsNoTracking()
                                 .Where(x => x.TournamentId == tournamentId && x.IsDeleted == false && x.IsActive == true)
                                 .Include(x => x.Venue)
                                 .ToListAsync(); 
        }

        public IQueryable<TournamentVenue> GetAll()
        {
            return _context.TournamentVenue.AsNoTracking().AsQueryable();
        }

        public async Task<List<TournamentVenue>> CreateTournamentVenueListAsync(List<TournamentVenue> tournamentVenueList)
        {
            await _context.AddRangeAsync(tournamentVenueList);
            await _context.SaveChangesAsync();
            return tournamentVenueList;
        }
        public async Task<List<TournamentStatkeeper>> CreateTournamentStatkeeperListAsync(List<TournamentStatkeeper> tournamentStatkeeperList)
        {
            await _context.AddRangeAsync(tournamentStatkeeperList);
            await _context.SaveChangesAsync();
            return tournamentStatkeeperList;
        }
    }
}
