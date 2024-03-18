using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Event;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
namespace Ensuranx.Infrastructure.Repositories
{
    public class EventTeamRepository : IEventTeamRepository
    {
        private readonly ApplicationDbContext _context;
        public EventTeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<EventTeam>> GetEventTeamByEventIdAsync(long eventId)
        {
            var query = await _context.EventTeam.Where(x => x.EventId == eventId)
                              .Include(x => x.EventCollectionTeam)
                              .Include(x => x.EventCollectionTeam.EventCollection)
                              .Include(x => x.EventCollectionTeam.EventCollection.EventType)
                              .Include(x => x.EventCollectionTeam.Team)
                              .Include(x => x.EventCollectionTeam.EventCollection.Venue.Branch)
                              .Include(x => x.EventCollectionTeam.EventCollection.UserInfo)
                              .Include(x => x.EventCollectionTeam.EventCollection.Activity.ActivityCategory)
                              .Include(x => x.EventCollectionTeam.EventCollection.EventType)
                              .Include(x => x.EventStatus)
                              .ToListAsync();
            return query;
        }
        public async Task<EventTeam> GetEventTeamByEventCollectionTeamIdAsync(long eventCollectionTeamId)
        {
            return await _context.EventTeam.AsNoTracking().Where(x => x.EventCollectionTeamId == eventCollectionTeamId && 
            x.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Win.ToString().ToLower() &&
            x.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower() &&
            x.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower() 
            )
                .Include(x => x.EventStatus)
                .Include(x => x.Event)
                .Include(x => x.EventCollectionTeam.EventCollection.Activity.ActivityCategory)
                .FirstOrDefaultAsync();
        }

        public async Task<EventTeam> GetEventTeamByEventCollectionIdAndTeamIdAsync(long eventCollectionId, long teamId)
        {
            return await _context.EventTeam.AsNoTracking().Where(x => x.EventCollectionTeam.EventCollectionId == eventCollectionId && x.EventCollectionTeam.TeamId == teamId).Include(x => x.EventStatus).FirstOrDefaultAsync();
        }
        public async Task<EventTeam> GetEventTeamByEventCollectionIdAndPlayerIdAsync(long eventCollectionId, long playerId, string eventStatus)
        {
            return await (from eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                          join teamMemberTbl in _context.TeamMember.AsNoTracking()
                          on eventCollectionTeamTbl.TeamId equals teamMemberTbl.TeamId
                          join evenTeamTbl in _context.EventTeam.AsNoTracking()
                          on eventCollectionTeamTbl.Id equals evenTeamTbl.EventCollectionTeamId
                          where eventCollectionTeamTbl.EventCollectionId == eventCollectionId && teamMemberTbl.PlayerId == playerId && teamMemberTbl.IsDeleted != true && teamMemberTbl.IsActive != false &&
                          teamMemberTbl.IsAccepted != false && teamMemberTbl.IsSubstituteOrDisqualify != true && evenTeamTbl.EventStatus.Name.ToLower() == eventStatus
                          select evenTeamTbl).FirstOrDefaultAsync();
        }
        public async Task<List<TeamMemberMatchesWin>> GetEventTeamWinByTeamMemberIdList(List<long> teamMemberList)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking().Include(x => x.UserInfo)
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.EventCollectionTeam)
                               on teamMemberTbl.TeamId equals eventTeamTbl.EventCollectionTeam.TeamId
                               where teamMemberList.Contains(teamMemberTbl.PlayerId.Value)
                               select new TeamMemberMatchesWin
                               {
                                   TeamMember = teamMemberTbl,
                                   EventTeam = eventTeamTbl
                               }).ToListAsync();
            return query;
        }
        public async Task<List<EventTeam>> UpdateEventTeamAsync(List<EventTeam> eventTeamList)
        {
            _context.EventTeam.UpdateRange(eventTeamList);
            await _context.SaveChangesAsync();
            return eventTeamList;
        }
        public async Task<MatchSummaryRespList> getMatchSummary(long eventId, long userId, long teamOneScore, long teamTwoScore, long statId, long teamMemberId)
        {
            var results = await (from eventLog in _context.EventLog.Where(x => x.EventId == eventId)
                                 join events in _context.Event
                                 on eventLog.EventId equals events.Id
                                 join teamMember in _context.TeamMember.Where(x => x.Id == teamMemberId)
                                 on eventLog.TeamMemberId equals teamMember.Id
                                 join stat in _context.Stat
                                 on eventLog.StatId equals stat.Id
                                 join imageTbl in _context.Image.AsNoTracking()
                                 .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                             x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                 on teamMember.PlayerId equals imageTbl.TableId into newFullImage
                                 from imageTblFull in newFullImage.DefaultIfEmpty()
                                 where stat.Id == statId
                                 select new MatchSummaryRespList
                                 {
                                     teamId = teamMember.TeamId,
                                     teamName = teamMember.Team.Name,
                                     teamColor = teamMember.Team.Colour,
                                     playerImage = imageTblFull != null ? imageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     playerId = (long)teamMember.PlayerId,
                                     playerName = teamMember.UserInfo.Name,
                                     isSucessfull = eventLog.IsSucessfull,
                                     stateName = eventLog.Stat.Name,
                                     logDateTime = eventLog.CreatedDateTime,
                                     teamOneScore = teamOneScore,
                                     teamTwoScore = teamTwoScore,
                                 }).FirstOrDefaultAsync(); ;
            return results;
        }
        // OverRidding the Function
        public async Task<List<MatchSummaryRespList>> getMatchSummary(long eventId, long userId)
        {
            var results = await (from eventLog in _context.EventLog.Where(x => x.EventId == eventId)
                                 join events in _context.Event
                                 on eventLog.EventId equals events.Id
                                 join teamMember in _context.TeamMember
                                 on eventLog.TeamMemberId equals teamMember.Id
                                 join stat in _context.Stat
                                 on eventLog.StatId equals stat.Id
                                 select new MatchSummaryRespList
                                 {
                                     teamId = teamMember.TeamId,
                                     teamName = teamMember.Team.Name,
                                     playerId = (long)teamMember.PlayerId,
                                     playerName = teamMember.UserInfo.Name,
                                     isSucessfull = eventLog.IsSucessfull,
                                     stateName = eventLog.Stat.Name,
                                     logDateTime = eventLog.CreatedDateTime,
                                 }).ToListAsync();
            return results;
        }
        public Task<DateTime> GetStartEventDateTime(long eventId)
        {
            return _context.EventLog.Where(x => x.EventId == eventId)
                                    .Select(x => x.CreatedDateTime)
                                    .FirstOrDefaultAsync();
        }
        public async Task<DateTime> GetEndEventDateTime(long eventId)
        {
            DateTime lastDateTime = await _context.EventLog
                     .Where(x => x.EventId == eventId)
                     .Select(x => x.CreatedDateTime)
                     .OrderByDescending(x => x)
                     .FirstOrDefaultAsync();
            return lastDateTime;
        }
        public Task<List<long>> getEventlogIds(long eventId, DateTime startdate, DateTime endDate)
        {
            return _context.EventLog.Where(x => x.EventId == eventId && x.CreatedDateTime >= startdate && x.CreatedDateTime <= endDate)
                                    .Select(x => x.Id)
                                    .ToListAsync();
        }
        public async Task<List<EventLogResp>> getEventlog(BasicFilter paginationFilter, List<long> eventLogId)
        {
            var Results = await (from eventlog in _context.EventLog
                                 where eventLogId.Contains(eventlog.Id)
                                 select new EventLogResp
                                 {
                                     eventIdTemp = eventlog.Id,
                                     TeamMemberId = eventlog.TeamMemberId,
                                     PlayerId = (long)eventlog.TeamMember.PlayerId,
                                     TeamId = eventlog.TeamMember.TeamId,
                                     EventId = eventlog.EventId,
                                     statid = eventlog.StatId,
                                     score = _context.EventLog.Where(x => x.Id == eventlog.Id && x.IsFix != true
                                                     && x.IsSucessfull != false &&
                                                     x.Stat.StatType.Name.ToLower() == Domain.Enums.Enum.StatType.HitMiss.ToString().ToLower()).
                                                     Select(x => x.Stat.Value).Sum()
                                 }).OrderBy(x => x.eventIdTemp)
                                   .ToListAsync();
            return Results;
        }
        public async Task<List<EventTeams>> getEventTeam(long eventId)
        {
            var results = (from eventTeam in _context.EventTeam.Where(x => x.EventId == eventId)
                           join ect in _context.EventCollectionTeam
                           on eventTeam.EventCollectionTeamId equals ect.Id
                           select new EventTeams
                           {
                               TeamId = eventTeam.EventCollectionTeam.TeamId,
                           }).ToList();
            return results;
        }

        public async Task<EventTeam> GetActiveEventInEventCollection(long eventCollectionId)
        {
            return await (from evenCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                          join evenTeamTbl in _context.EventTeam.AsNoTracking()
                          on evenCollectionTeamTbl.Id equals evenTeamTbl.EventCollectionTeamId

                          where evenCollectionTeamTbl.EventCollectionId == eventCollectionId && evenTeamTbl.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Active.ToString().ToLower()
                          select evenTeamTbl
                          ).FirstOrDefaultAsync();
        }
    }
}
