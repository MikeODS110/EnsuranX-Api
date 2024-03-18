using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Team;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Team;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Cryptography;
using static Ensuranx.Domain.Common.Errors.Errors;

namespace Ensuranx.Infrastructure.Repositories
{
    public class TeamMemberRepository : ITeamMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamMemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<long> GetTeamsIdByPlayerIdList(long userId) 
        {
            return _context.TeamMember.AsNoTracking().Where(x => x.PlayerId == userId).Select(x => x.TeamId).AsQueryable();
        }

        public async Task<List<TeamMember>> GetTeamMembersList(long userId,List<long> teamIdList)
        {
            return await _context.TeamMember
                         .Where(x => teamIdList.Contains(x.TeamId) && x.IsDeleted != true && x.IsActive != false && x.IsAccepted != false)
                         .Include(x => x.Team)
                         .Include(x => x.UserInfo)
                         .Include(x => x.TeamMemberRole)
                         .ToListAsync();
        }

        public async Task<List<TeamMember>> GetTeamMembersAllPlayedList(long userId, List<long> teamIdList)
        {
            return await _context.TeamMember
                         .Where(x => teamIdList.Contains(x.TeamId) && x.IsDeleted != true && x.IsActive != false && x.IsAccepted != false && x.IsJoinedCollection != false)
                         .Include(x => x.Team)
                         .Include(x => x.UserInfo)
                         .Include(x => x.TeamMemberRole)
                         .ToListAsync();
        }

        public async Task<List<TeamMember>> GetTeamMembersAllList(long userId, List<long> teamIdList)
        {
            return await _context.TeamMember
                         .Where(x => teamIdList.Contains(x.TeamId) && x.IsAccepted != false)
                         .Include(x => x.Team)
                         .Include(x => x.UserInfo)
                         .Include(x => x.TeamMemberRole)
                         .ToListAsync();
        }

        public async Task<List<TeamMember>> GetTeamMembersDeletedList(long userId, List<long> teamIdList)
        {
            return await _context.TeamMember
                         .Where(x => teamIdList.Contains(x.TeamId) && x.IsAccepted != false && x.IsDeleted != true && x.IsActive != false && x.IsSubstituteOrDisqualify != true && x.IsJoinedCollection != false)
                         .Include(x => x.Team)
                         .Include(x => x.UserInfo)
                         .Include(x => x.TeamMemberRole)
                         .ToListAsync();
        }


        public async Task<List<TeamMember>> DeleteTeamMemberList(List<TeamMember> teamMemberList)
        {
            foreach (TeamMember teamMember in teamMemberList)
            {
                teamMember.IsDeleted = true;
                teamMember.IsActive = false;
                teamMember.IsSubstituteOrDisqualify = false;
            }

            _context.TeamMember.UpdateRange();
            await _context.SaveChangesAsync();

            return teamMemberList;
        }

        public async Task<TeamMember> CheckIfThisPlayerIsPresentInWhichTeamList(long playerId, List<long> teamIdList)
        {
            return await _context.TeamMember.AsNoTracking().Where(x => teamIdList.Contains(x.TeamId) && x.PlayerId == playerId && x.IsJoinedCollection != false).FirstOrDefaultAsync();
        }

        public async Task<TeamMember> GetTeamMemberByTeamIdFirstOrDefault(long teamId)
        {
            return await _context.TeamMember.AsNoTracking().Include(x => x.Team.TeamType).Where(x => x.TeamId == teamId && x.IsDeleted != true && x.IsActive != false && x.IsAccepted != false).FirstOrDefaultAsync();
        }

        public async Task<TeamMember> GetTeamMemberByTeamIdNotSubstituteFirstOrDefault(long teamId)
        {
            return await _context.TeamMember.AsNoTracking().Include(x => x.Team.TeamType).Where(x => x.TeamId == teamId && x.IsDeleted != true && x.IsActive != false && x.IsAccepted != false && x.IsSubstituteOrDisqualify != true).FirstOrDefaultAsync();
        }

        public async Task<List<TeamMember>> GetTeamMembersListByTeamId(long teamId)
        {
            return await _context.TeamMember.AsNoTracking().Where(x => x.TeamId == teamId && x.IsAccepted != false && x.IsActive != false && x.IsDeleted != true)
                        .Include(x => x.Team)
                        .Include(x => x.UserInfo)
                        .Include(x => x.TeamMemberRole)
                        .ToListAsync();
        }

        public async Task<List<TeamMember>> GetTeamMembersListWhichPlayedWithThisTeamIdLastMatchCount(long userId, int lastMatchCount, List<long> totalTeamMemberIdList)
        {
            var teamIdList = _context.TeamMember.AsNoTracking().Where(x => x.PlayerId == userId).Select(x => x.TeamId).ToList();

            var eventTeamIdList = _context.EventTeam.AsNoTracking().Where(x => teamIdList.Contains(x.EventCollectionTeam.TeamId)).Select(x => x.EventCollectionTeam.TeamId).ToList();

            var teamMemberList = await _context.TeamMember.AsNoTracking().Include(x => x.UserInfo)
                                                        .Where(x => eventTeamIdList.Contains(x.TeamId) && x.IsAccepted != false && x.IsDeleted != true &&
                                                        x.IsActive != false && !totalTeamMemberIdList.Contains(x.PlayerId.Value))
                                                        .Select(x => new TeamMember { PlayerId = x.PlayerId.Value,UserInfo = x.UserInfo })
                                                        .Distinct()
                                                        .ToListAsync();
            return teamMemberList;
        }

        public async Task<TeamMember> GetCaptainOfTheTeam(long teamId)
        {
            return await _context.TeamMember.AsNoTracking()
                        .Where(x => x.TeamId == teamId && x.TeamMemberRole.Name.ToLower() == Domain.Enums.Enum.TeamMemberRole.Captain.ToString().ToLower() &&
                        x.IsAccepted != false && x.IsDeleted != true && x.IsActive != false)
                        .FirstOrDefaultAsync();
        }

        public async Task<List<PlayerDetails>> GetAllPlayerAsyncByTeamId(long teamId, string sSearch)
        {
            return await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                          join imageTbl in _context.Image.AsNoTracking()
                          .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                          .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                          on teamMemberTbl.PlayerId equals imageTbl.TableId into imageFullCheck
                          from imageTblFull in imageFullCheck.DefaultIfEmpty()

                          where teamMemberTbl.TeamId == teamId && teamMemberTbl.IsAccepted != false
                          where teamMemberTbl.TeamMemberRole.Name.ToLower() != Domain.Enums.Enum.TeamMemberRole.Captain.ToString().ToLower()
                          where !string.IsNullOrEmpty(sSearch) ? teamMemberTbl.UserInfo.Name.ToLower().Contains(sSearch) : teamMemberTbl.Id != 0


                          select new PlayerDetails
                          {
                              Id = teamMemberTbl.PlayerId.Value,
                              Name = teamMemberTbl.UserInfo.Name,
                              Role  = teamMemberTbl.TeamMemberRole.Name,
                              Path = imageTblFull != null ? imageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                          }).ToListAsync();
        }

        public int GetTeamMemberRequestAcceptedCount(long teamId)
        {
            return _context.TeamMember.AsNoTracking().Where(x => x.TeamId == teamId && x.IsAccepted != false).Count();
        }

        public int GetTeamMemberCountByTeamId(long teamId)
        {
            return _context.TeamMember.AsNoTracking().Count(x => x.TeamId == teamId && x.IsActive != false && x.IsDeleted != true && x.IsSubstituteOrDisqualify != true && x.IsJoinedCollection != false);
        }

        public async Task<List<TeamMemberMatchesWon>> GetTeamMembersMatchesWonList(long userId, List<long> teamMembersIdList)
        {
            var query = await (from eventTeamCollectionTbl in _context.EventCollectionTeam.AsNoTracking()
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus)
                               on eventTeamCollectionTbl.Id equals eventTeamTbl.EventCollectionTeamId

                               join teamMemberTbl in _context.TeamMember.AsNoTracking()
                               on eventTeamCollectionTbl.TeamId equals teamMemberTbl.TeamId
                               
                               where teamMembersIdList.Contains(teamMemberTbl.PlayerId.Value) && 
                               eventTeamTbl.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()
                               group teamMemberTbl by teamMemberTbl.PlayerId into g
                               select new TeamMemberMatchesWon
                               {
                                   TeamMemberId = g.Key.Value,
                                   MatchesWonCount = g.Count(),
                               }).ToListAsync();
            return query;
        }
        public async Task<TeamMember> GetTeamMemberRequestAcceptedByTeamIdAndPlayerId(long teamId, long playerId)
        {
            return await _context.TeamMember.AsNoTracking().Include(x => x.Team.TeamType)
                         .Where(x => x.TeamId == teamId && x.PlayerId == playerId)
                         .FirstOrDefaultAsync();
        }

        public async Task<List<TeamMember>> GetTeamMemberRequestByTeamId(long teamId)
        {
            return await _context.TeamMember.AsNoTracking()
                         .Where(x => x.TeamId == teamId && x.IsDeleted != true && x.IsActive != false)
                         .ToListAsync();
        }

        public async Task<TeamMember> GetTeamMemberRequestAcceptedButDeletedByTeamIdAndPlayerId(long teamId, long playerId)
        {
            return await _context.TeamMember.AsNoTracking()
                         .Where(x => x.TeamId == teamId && x.PlayerId == playerId && x.IsAccepted != false && x.IsDeleted != false && x.IsActive != true)
                         .FirstOrDefaultAsync();
        }

        public async Task<TeamMember> CheckIfPlayerIsAMemberOfAnyTeamInEventCollectionById(long playerId, long eventCollectionId)
        {
            return await (from eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                          join teamMemberTbl in _context.TeamMember.AsNoTracking()
                          on eventCollectionTeamTbl.TeamId equals teamMemberTbl.TeamId

                          where eventCollectionTeamTbl.EventCollectionId == eventCollectionId && teamMemberTbl.PlayerId == playerId

                          select teamMemberTbl).FirstOrDefaultAsync();
        }

        public async Task<TeamMember> GetTeamMemberRequestNotAcceptedByTeamIdAndPlayerId(long playerId,long teamId)
        {
            return await _context.TeamMember.AsNoTracking()
                         .Where(x => x.TeamId == teamId && x.PlayerId == playerId && x.IsAccepted != true)
                         .FirstOrDefaultAsync();
        }

        public async Task<List<TeamMember>> GetTeamMemberListRequestNotAcceptedByTeamId(long teamId)
        {
            return await _context.TeamMember.AsNoTracking().Include(x => x.UserInfo)
                         .Where(x => x.TeamId == teamId && x.IsAccepted != true && x.IsDeleted != true && x.IsActive != false)
                         .ToListAsync();
        }

        public async Task<TeamMember> checkCaption(long playerId)
        {
            return await _context.TeamMember.AsNoTracking().Where(tm => tm.PlayerId == playerId).FirstOrDefaultAsync();
        }
        public async Task<List<GetAllAvgResp>> GetAvgTournamentEvents(long userId, long tournamentId, long venueId)
        {
            var results = await (from eventTeamMemberStatResult in _context.EventTeamMemberStatResult
                                 .Include(x => x.TeamMember)
                                 .Include(x => x.TeamMember.UserInfo)
                                 .Where(x => x.TeamMember.UserInfo.Id == userId)
                               

                                /* join events in _context.Event
                                 on eventTeamMemberStatResult.EventId equals events.Id

                                 join eventTeam in _context.EventTeam
                                 on events.Id equals eventTeam.EventId

                                 join eventTeamCollection  in _context.EventCollectionTeam
                                 on eventTeam.EventCollectionTeamId equals eventTeamCollection.Id

                                 join eventCollection in _context.EventCollection
                                 on eventTeamCollection.EventCollectionId equals eventCollection.Id

                                 join venue in _context.Venue
                                 .Where(x => venueId != 0 ? x.Id == venueId : x.Id != 0)
                                 on eventCollection.VenueId equals venue.Id*/

                                 select new GetAllAvgResp
                                 {
                                     AvgAssit = eventTeamMemberStatResult.Assist,
                                     AvgBlock = eventTeamMemberStatResult.Block,
                                     AvgMvpPerc = (long)eventTeamMemberStatResult.MvpPerc,
                                     AvgRebound = eventTeamMemberStatResult.Rebound,
                                     AvgStealWinPercent = eventTeamMemberStatResult.WinPerc,
                                     AvgTotalPoint = eventTeamMemberStatResult.TotalPoint,
                                     AvgTotalScore = eventTeamMemberStatResult.TotalScore,
                                     AvgWinPerc = eventTeamMemberStatResult.WinPerc,
                                     AvgWinScore = eventTeamMemberStatResult.WinScore,
                                     AvgSteal = eventTeamMemberStatResult.Steal
                                 }).ToListAsync();

            return results;
        }
        public async Task<List<GetVenueEvent>> GetVenuesForAvg(long userId, long venueId)
        {
            var results = await (from venue in _context.Venue
                                .Where(x=>venueId!=0?x.Id==venueId:x.Id!=0)

                                 join branch in _context.Branch
                                 on venue.BranchId equals branch.Id


                                 join eventCollection in _context.EventCollection
                                 on venue.Id equals eventCollection.VenueId


                                 join eventCollectionTeam in _context.EventCollectionTeam
                                 on eventCollection.Id equals eventCollectionTeam.EventCollectionId

                                 join team in _context.Team
                                 on eventCollectionTeam.TeamId equals team.Id

                                 join teamMember in _context.TeamMember
                                 on team.Id equals teamMember.TeamId


                                 join userInfo in _context.UserInfo.Where(x => x.Id == userId)
                                 on teamMember.PlayerId equals userInfo.Id


                                 join eventTeam in _context.EventTeam
                                 on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                 join events in _context.Event
                                 on eventTeam.EventId equals events.Id

                                 select new GetVenueEvent
                                 {
                                     venueId = venue.Id,
                                     eventId = events.Id
                                 }).ToListAsync();
            return results;
        }


        public async Task<List<GetAllAvgResp>> GetAvgOnEvent(long userId, List<long> eventListId)
        {
         


            var results = await (from eventTeamMemberStatResult in _context.EventTeamMemberStatResult
                                .Include(x => x.TeamMember)
                                .Include(x => x.TeamMember.UserInfo)
                                .Where(x => x.TeamMember.UserInfo.Id == userId)

                             
                                 where eventListId.Contains(eventTeamMemberStatResult.EventId)

                                 join @event in _context.Event
                                 on eventTeamMemberStatResult.EventId equals @event.Id


                                 select new GetAllAvgResp
                                 {
                                     AvgAssit = eventTeamMemberStatResult.Assist,
                                     AvgBlock = eventTeamMemberStatResult.Block,
                                     AvgMvpPerc = (long)eventTeamMemberStatResult.MvpPerc,
                                     AvgRebound = eventTeamMemberStatResult.Rebound,
                                     AvgStealWinPercent = eventTeamMemberStatResult.WinPerc,
                                     AvgTotalPoint = eventTeamMemberStatResult.TotalPoint,
                                     AvgTotalScore = eventTeamMemberStatResult.TotalScore,
                                     AvgWinPerc = eventTeamMemberStatResult.WinPerc,
                                     AvgWinScore = eventTeamMemberStatResult.WinScore,
                                     AvgSteal = eventTeamMemberStatResult.Steal
                                 }).ToListAsync();
            return results;
        }

         public async Task<long> GetTeamMemberVsTournament(long tournamentId,long userId)
            {
            var results = await (from tournament in _context.Tournament.Where(x => x.Id == tournamentId)
                                join tournamentTeam in _context.TournamentTeam
                                on tournament.Id equals tournamentTeam.TournamentId

                                join team in _context.Team
                                on tournamentTeam.TeamId equals team.Id

                                join teamMember in _context.TeamMember.Where(x => x.PlayerId == userId)
                                on team.Id equals teamMember.TeamId

                                select team.Id).FirstOrDefaultAsync();

                    return results;
            }


        public List<long> GetEventVsTournament(long team, long userId)
        {

            var query =       (from events in _context.Event
                               join eventTeam in _context.EventTeam
                               on events.Id equals eventTeam.EventId

                               join eventTeamCollection in _context.EventCollectionTeam.Where(x => x.TeamId == team)
                               on eventTeam.EventCollectionTeamId equals eventTeamCollection.Id

                               join eventCollection in _context.EventCollection
                               on eventTeamCollection.EventCollectionId equals eventCollection.Id

                               join eventCollectionTournament in _context.TournamentEventCollection
                               on eventCollection.Id equals eventCollectionTournament.EventCollectionId

                               join tournament in _context.Tournament
                               on eventCollectionTournament.TournamentId equals tournament.Id

                               select events.Id
                              ).ToList();

            //var results = await (from tournament in _context.Tournament.Where(x => x.Id == team)
            //                     join tournamentEC in _context.TournamentEventCollection
            //                     on tournament.Id equals tournamentEC.TournamentId

            //                     join EC in _context.EventCollection
            //                     on tournamentEC.EventCollectionId equals EC.Id

            //                     join ECTeam in _context.EventCollectionTeam
            //                     on EC.Id equals ECTeam.EventCollectionId

            //                     join eventTeam in _context.EventTeam
            //                     on ECTeam.Id equals eventTeam.EventCollectionTeamId

            //                     join events in _context.Event
            //                     on eventTeam.EventId equals events.Id

            //                     select events.Id).ToListAsync();

            return query;
        }


        public async Task<List<long>> GetEventsaByEventType(long eventType, long userId)
        {
            var results = await (from ec in _context.EventCollection.Where(x => x.EventTypeId == eventType)
                                 join ect in _context.EventCollectionTeam
                                 on ec.Id equals ect.EventCollectionId

                                 join team in _context.Team
                                 on ect.TeamId equals team.Id

                                 join teamMember in _context.TeamMember
                                 on team.Id equals teamMember.TeamId

                                 where teamMember.PlayerId == userId

                                 join eventTeam in _context.EventTeam
                                 on ect.Id equals eventTeam.EventCollectionTeamId

                                 join events in _context.Event
                                 on eventTeam.EventId equals events.Id


                                 select ec.Id).ToListAsync();
            return results;

        }
        //public GetAllAvgResp GetAvgCal(List<GetAllAvgResp> getAllAvgResp)
        //{
        //    var Avgs = new GetAllAvgResp();

        //    Avgs = getAllAvgResp.Select(x=>x.AvgAssit).ToList();

        //}

        public async Task<List<TeamMember>> GetTeamMemberListByEventId(long eventId)
        {
            return await (from eventTeamTbl in _context.EventTeam.AsNoTracking()
                          join teamMemberTbl in _context.TeamMember.AsNoTracking().Include(x => x.Team).Include(x => x.UserInfo)
                          on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                          where eventTeamTbl.EventId == eventId
                          select teamMemberTbl).ToListAsync();
        }

        public async Task<List<string>> getTeamJersey(long teamId,long userId)
        {
            var results = await _context.TeamMember.Where(x => x.Team.Id == teamId && x.PlayerId!= userId)
                                                   .Select(x => x.JerseyNo)
                                                   .OrderBy(jerseyNo => jerseyNo).ToListAsync();

            return results;
        }
    }
}
