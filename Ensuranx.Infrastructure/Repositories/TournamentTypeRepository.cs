using Ensuranx.Application.Interfaces;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories;

public class TournamentTypeRepository : ITournamentTypeRepository
{
    private readonly ApplicationDbContext _context;

    public TournamentTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Ensuranx.Domain.Entities.TournamentType> GetTournamentTypeByNameAsync(string name)
    {
        return await _context.TournamentType.AsNoTracking().Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
    }
}
