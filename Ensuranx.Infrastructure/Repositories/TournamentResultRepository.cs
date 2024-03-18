using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories;

public class TournamentResultRepository : ITournamentResultRepository
{
    private readonly ApplicationDbContext _context;

    public TournamentResultRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetTournamentResult>> GetTournamentResultByTournamentIdAsync(long tournamentId, long userId)
    {
        var query = await (from tournamentTbl in _context.Tournament.AsNoTracking()
                           join tournamentResultTbl in _context.TournamentResult.AsNoTracking().Include(x => x.Team).Include(x=> x.Position)
                           on tournamentTbl.Id equals tournamentResultTbl.TournamentId

                           where tournamentTbl.Id == tournamentId

                           select new GetTournamentResult
                           {
                               TeamName = tournamentResultTbl.Team.Name,
                               Colour = tournamentResultTbl.Team.Colour,
                               TeamPlayerCount = _context.TeamMember.AsNoTracking().Where(x => x.TeamId == tournamentResultTbl.TeamId).Count(),
                               Win = _context.EventTeam.AsNoTracking()
                                     .Where(x => x.EventCollectionTeam.TeamId == tournamentResultTbl.TeamId && 
                                     x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()).Count(),
                               lose = _context.EventTeam.AsNoTracking()
                                     .Where(x => x.EventCollectionTeam.TeamId == tournamentResultTbl.TeamId &&
                                     x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower()).Count(),
                               Tie = _context.EventTeam.AsNoTracking()
                                     .Where(x => x.EventCollectionTeam.TeamId == tournamentResultTbl.TeamId &&
                                     x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower()).Count(),
                              Position = tournamentResultTbl.Position.Name,
                           }).ToListAsync();
        return query;
    }
}
