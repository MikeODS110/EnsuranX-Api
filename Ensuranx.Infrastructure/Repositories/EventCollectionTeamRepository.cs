using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Event;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class EventCollectionTeamRepository : IEventCollectionTeamRepository
    {
        private readonly ApplicationDbContext _context;

        public EventCollectionTeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<EventCollectionTeam>> GetEventCollectionTeamList(CreateEvent createEvent)
        {
            List<EventCollectionTeam> eventCollectionTeamList = new List<EventCollectionTeam>();

            if (createEvent.EventId != 0)
            {
                eventCollectionTeamList = await (from eventTeamTbl in _context.EventTeam
                                                 join eventCollectionTeamTbl in _context.EventCollectionTeam.Include(x => x.EventCollection.Activity.ActivityCategory)
                                                 on eventTeamTbl.EventCollectionTeamId equals eventCollectionTeamTbl.Id

                                                 where createEvent.teamIdList.Contains(eventCollectionTeamTbl.TeamId)
                                                 where createEvent.EventId != 0 ? eventTeamTbl.EventId == createEvent.EventId : eventCollectionTeamTbl.Id != 0

                                                 select eventCollectionTeamTbl
                    ).ToListAsync();
            }
            else
            {
                eventCollectionTeamList = await _context.EventCollectionTeam.AsNoTracking().Where(x => createEvent.teamIdList.Contains(x.TeamId)).Include(x => x.EventCollection.Activity.ActivityCategory).ToListAsync();
            }
            return eventCollectionTeamList;
        }

        public async Task<List<EventCollectionTeam>> CreateEventCollectionTeamList(List<EventCollectionTeam> eventCollectionList)
        {
            await _context.EventCollectionTeam.AddRangeAsync(eventCollectionList);
            await _context.SaveChangesAsync();
            return eventCollectionList;
        }

        public async Task<string> GetMaxSequenceEventCollectionTeamByEventCollectionId(long eventCollectionId)
        {
            var res = await _context.EventCollectionTeam.AsNoTracking().Where(x => x.EventCollectionId == eventCollectionId).Select(x => x.Sequence).Where(x => x != "A").MaxAsync();
            return res;
        }

        public async Task<List<long>> GetTeamIdListForAllTeamInTheSameTeamEventCollection(long teamId)
        {
            var getEventCollection = await _context.EventCollectionTeam.AsNoTracking().Where(x => x.TeamId == teamId).Select(x => x.EventCollectionId).FirstOrDefaultAsync();
            return await _context.EventCollectionTeam.AsNoTracking().Where(x => x.EventCollectionId == getEventCollection).Select(x => x.TeamId).ToListAsync();
        }

        public async Task<long> GetEventCollectionIdByTeamId(long teamId)
        {
            return await _context.EventCollectionTeam.AsNoTracking().Where(x => x.TeamId == teamId).Select(x => x.EventCollectionId).FirstOrDefaultAsync();
        }

        public async Task<EventCollectionTeam> GetEventCollectionIdByTeamIdAndEventCollectionIdAsync(long teamId, long eventCollectionId)
        {
            return await _context.EventCollectionTeam.Where(x => x.EventCollectionId == eventCollectionId && x.TeamId == teamId).FirstOrDefaultAsync();
        }
    }
}
