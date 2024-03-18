using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Event;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Ensuranx.Infrastructure.Repositories;

public class EventTeamMemberStatResultRepository : IEventTeamMemberStatResultRepository
{
    private readonly ApplicationDbContext _context;

    public EventTeamMemberStatResultRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventTeamMemberStatResult>> CreateRange(List<EventTeamMemberStatResult> eventTeamMemberStatResultList)
    {
        await _context.EventTeamMemberStatResult.AddRangeAsync(eventTeamMemberStatResultList);
        await _context.SaveChangesAsync();
        return eventTeamMemberStatResultList;
    }
    
    
    public async Task<List<EventTeamMemberStatResult>> UpdateRange(List<EventTeamMemberStatResult> eventTeamMemberStatResultList)
    {
        _context.EventTeamMemberStatResult.UpdateRange(eventTeamMemberStatResultList);
        await _context.SaveChangesAsync();
        return eventTeamMemberStatResultList;
    }

    public async Task<List<EventTeamMemberStatResult>> GetTeamMembersPreviousDataAsync(List<long> teamMemberIdList)
    {
        var query = await _context.EventTeamMemberStatResult.AsNoTracking()
                          .Where(x => teamMemberIdList.Contains(x.TeamMember.PlayerId.Value) && x.IsActive != false && x.IsDeleted != true)
                          .Include(x => x.TeamMember)
                          .ToListAsync();
        return query;
    }

    public async Task<List<EventTeamMemberStatResult>> GetEventTeamMemberStatResultByEventIdAsync(long eventId)
    {
        return await _context.EventTeamMemberStatResult.AsNoTracking()
                    .Where(x => x.EventId == eventId && x.IsDeleted != true && x.IsActive != false)
                    .Include(x => x.TeamMember)
                    .ToListAsync();
    }

    public async Task<List<EventPlayedByPlayerCount>> GetEventTeamMemberStatResultByEventIdAsync(List<long> playerIdList)
    {
        var res = await (from userInfoTbl in _context.UserInfo.AsNoTracking().Where(x => playerIdList.Contains(x.Id))
                         join eventTeamMemberStatResultTbl in _context.EventTeamMemberStatResult.AsNoTracking()
                         on userInfoTbl.Id equals eventTeamMemberStatResultTbl.TeamMember.PlayerId.Value
                         group eventTeamMemberStatResultTbl by eventTeamMemberStatResultTbl.TeamMember.PlayerId.Value into newGrouped
                         select new EventPlayedByPlayerCount
                         {
                             PlayerId = newGrouped.First().TeamMember.PlayerId.Value,
                             GamesPlayedCount = newGrouped.Count(),
                         }).ToListAsync();

        return res;
    }
}
