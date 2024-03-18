using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories;

public class TeamActivityRepository : ITeamActivityRepository
{
    private readonly ApplicationDbContext _context;

    public TeamActivityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeamActivity> GetTeamActivityByTeamId(long teamId)
    {
        return await _context.TeamActivity.Where(x => x.TeamId == teamId).FirstOrDefaultAsync();
    }
}
