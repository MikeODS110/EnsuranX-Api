using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Event;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class EventLogRepository : IEventLogRepository
    {
        private readonly ApplicationDbContext _context;

        public EventLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EventLog> GetEventLogByEventMemberStatId(long eventId, long teamMemberId, long statId,bool isSuccess)
        {
            return await _context.EventLog.AsNoTracking()
                        .Where(x => x.EventId == eventId && x.TeamMemberId == teamMemberId && x.IsFix != true && x.IsPause == 0 && x.StatId == statId && x.IsActive != false && x.IsDeleted != true && x.IsSucessfull == isSuccess)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync();
        }

        public async Task<List<EventLog>> GetEventLogListByEventId(long eventId, long userId)
        {
            return await _context.EventLog.AsNoTracking()
                        .Where(x => x.EventId == eventId && x.IsActive != false && x.IsDeleted != true)
                        .Include(x => x.Stat)
                        .ThenInclude(x => x.StatType)
                        .Include(x => x.TeamMember)
                        .ToListAsync();
        }
        public async Task<TeamScorePoints> getTeamScoreByPlayer(long eventId)
        {
            var results = await (from eventlog in _context.EventLog.Where(x=>x.EventId==eventId)


                                 select new TeamScorePoints
                                 {
                                     eventLogId= eventlog.EventId,
                                     teamId = eventlog.TeamMember.TeamId,
                                     //playerId = (long)eventlog.TeamMember.PlayerId,
                                     //score = _context.EventLog.Where(x => x.Id == eventId && x.IsFix != true
                                     //                && x.IsSucessfull != false &&
                                     //                x.Stat.StatType.Name.ToLower() == Domain.Enums.Enum.StatType.HitMiss.ToString().ToLower()).
                                     //                       Select(x => x.Stat.Value).Sum()
                                 }).FirstAsync();

            return results;


        }
    }
}
