using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Team;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class TournamentTeamRepository : ITournamentTeamRepository
    {
        private readonly ApplicationDbContext _context;

        public TournamentTeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<TournamentTeam> GetAll()
        {
            return _context.TournamentTeam.AsNoTracking().AsQueryable();
        }

        public async Task<TournamentTeam> CheckTournamentTeamRequestAsync(long tournamentId, long teamId)
        {
            var query = await _context.TournamentTeam.AsNoTracking()
                              .Where(x => x.TournamentId == tournamentId && x.TeamId == teamId && x.IsDeleted == false && x.IsActive == true)
                              .Include(x => x.Tournament)
                              .FirstOrDefaultAsync();
            return query;
        }

        public async Task<TournamentTeam> GetUserTeamJoinedInTheTournamentAsync(long tournamentId, long userId)
        {
            var query = await (from tournamentTeamTbl in _context.TournamentTeam.AsNoTracking()
                               join teamMemberTbl in _context.TeamMember.AsNoTracking()
                               on tournamentTeamTbl.TeamId equals teamMemberTbl.TeamId

                               where tournamentTeamTbl.TournamentId == tournamentId && teamMemberTbl.PlayerId == userId &&
                               teamMemberTbl.TeamMemberRole.Name.ToLower() == Domain.Enums.Enum.TeamMemberRole.Captain.ToString().ToLower()

                               select tournamentTeamTbl).FirstOrDefaultAsync();
            return query;
        }

        public int CheckTournamentTeamRequestAcceptedCountAsync(long tournamentId)
        {
            var query = _context.TournamentTeam.AsNoTracking()
                              .Where(x => x.TournamentId == tournamentId && x.IsDeleted == false && x.IsActive == true && x.IsAccepted == true)
                              .Count();
            return query;
        }

        public async Task<List<TournamentTeam>> GetTournamentTeamAllRequestAcceptedAsync(long tournamentId)
        {
            var query = await _context.TournamentTeam.AsNoTracking()
                              .Where(x => x.TournamentId == tournamentId && x.IsDeleted == false && x.IsActive == true && x.IsAccepted == true)
                              .Include(t => t.Team)
                              .ToListAsync();
            return query;
        }
        public async Task<List<RepeatedJersey>> GetRepeatedJersey(long teamId)
        {
            var repeatedJerseys = await (from teamMember in _context.TeamMember.Where(x => x.TeamId == teamId)
                                 join userInfo in _context.UserInfo

                                 on teamMember.PlayerId equals userInfo.Id
                                 select new RepeatedJersey
                                 {
                                     Id= (long)teamMember.PlayerId,
                                     Name=teamMember.UserInfo.Name,
                                     JerseyNo=teamMember.UserInfo.JerseyNo

                                 }
                                 ).ToListAsync();

            var filteredRepeatedJerseys = repeatedJerseys
            .GroupBy(j => j.JerseyNo)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();


            return filteredRepeatedJerseys;
        }
        public  long GetTournamentBusinesId(long tournamentId)
        {
            var result = _context.TournamentVenue.Where(x => x.TournamentId == tournamentId)
                    .Select(x => x.Venue.Branch.UserInfoId)
                    .FirstOrDefault();

            return (long)result;
        }
    }
}
