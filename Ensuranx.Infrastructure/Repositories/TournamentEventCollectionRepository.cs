using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories;

public class TournamentEventCollectionRepository : ITournamentEventCollectionRepository
{
    private readonly ApplicationDbContext _context;

    public TournamentEventCollectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TournamentEventCollection>> CreateTournamentEventCollectionList(List<TournamentEventCollection> tournamentEventCollectionList)
    {
        await _context.TournamentEventCollection.AddRangeAsync(tournamentEventCollectionList);
        await _context.SaveChangesAsync();
        return tournamentEventCollectionList;
    }

    public async Task<List<TournamentEventCollection>> GetTournamentEventCollectionByTournamentIdList(long tournamentId)
    {
        return await _context.TournamentEventCollection.AsNoTracking().Where(x => x.TournamentId == tournamentId).Include(x => x.Tournament).Include(x => x.EventCollection).Include(x => x.EventCollection.EventType).ToListAsync();
    }

    public async Task<TournamentEventCollection> GetTournamentEventCollectionByEventCollectionId(long eventCollectionId)
    {
        return await _context.TournamentEventCollection.AsNoTracking().Where(x => x.EventCollectionId == eventCollectionId).FirstOrDefaultAsync();
    }

    public async Task<long> GetTournamentEventCollectionIdByEventCollectionId(long eventCollectionId)
    {
        return await _context.TournamentEventCollection.AsNoTracking().Where(x => x.EventCollectionId == eventCollectionId).Select(x => x.TournamentId).FirstOrDefaultAsync();
    }
}
