using Azure;
using Google.Apis.Http;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Team;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Services.Event;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
//using Org.BouncyCastle.Bcpg;
// 
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Repositories
{
    public class BranchesVenuesRepository : IBranchesVenuesRepository
    {
        private readonly ApplicationDbContext _context;

        public BranchesVenuesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GetAllTeamsByVenueId>> GetAllTeamsByVenueId(List<long> venueid)
        {
            List<GetAllTeamsByVenueId> GetAllTeamsByVenueIdResponse = new List<GetAllTeamsByVenueId>();


            var results = await (from venue in _context.Venue


                                 join eventCollection in _context.EventCollection
                                 on venue.Id equals eventCollection.VenueId

                                 join eventCollectionTeam in _context.EventCollectionTeam
                                 on eventCollection.Id equals eventCollectionTeam.EventCollectionId

                                 join team in _context.Team
                                 on eventCollectionTeam.TeamId equals team.Id

                                 join teamMember in _context.TeamMember
                                 on team.Id equals teamMember.TeamId
                                 where team.Id == teamMember.TeamId

                                 where venueid.Contains(venue.Id)

                                 select new GetAllTeamsByVenueId
                                 {

                                     Id = team.Id,
                                     Name = team.Name,

                                 }
                             ).ToListAsync();

            return results;
        }
        /*public async Task<List<GetBranchesVenuesListWithoutTeams>> GetBranchesVenuesList(PaginationFilter paginationFilter, long userId)
        {
            List<GetBranchesVenuesListWithoutTeams> branchesVenuesListWithoutTeams = new List<GetBranchesVenuesListWithoutTeams>();
               
                var results = await (                   from venue in _context.Venue
                                                        join branch in _context.Branch 
                                                        on venue.BranchId equals branch.Id

                                                        join venueStateKeeper in _context.VenueStatkeeper
                                                        on venue.Id equals venueStateKeeper.VenueId

                                                        join user in _context.UserInfo.Include(x => x.User)
                                                        on venueStateKeeper.UserInfoId equals user.Id

                                                        join venueActivity in _context.VenueActivity
                                                        on venue.Id equals venueActivity.VenueId

                                                        join activity in _context.Activity
                                                        on venueActivity.ActivityId equals activity.Id

                                                        join eventCollection in _context.EventCollection
                                                        on venue.Id equals eventCollection.VenueId

                                                        join eventCollenctionTeam in _context.EventCollectionTeam
                                                        on eventCollection.Id equals eventCollenctionTeam.EventCollectionId

                                                        join eventTeam in _context.EventTeam
                                                        on eventCollenctionTeam.Id equals eventTeam.EventCollectionTeamId

                                                        join eventStatus in _context.EventStatus
                                                        on eventTeam.EventStatusId equals eventStatus.Id

                                                        select new GetBranchesVenuesListWithoutTeams
                                                        {
                                                            Id=venue.Id,
                                                            VenueName= venue.Name,
                                                            venueId = venue.Id,
                                                            BranchName=branch.Name,
                                                            StatskeeperName= user.User.UserName,
                                                            StatskeeperContact=user.User.Email,
                                                            Status=eventStatus.Name,
                                                            CurrentActivity=activity.Name,
                                                           
                                                             
                                                         }).ToListAsync();

            return results;
          
          }*/

        public async Task<List<BranchesVenuesResponseTemp>> GetBranchesVenuesListByBusiness(BasicFilter paginationFilter, long userId, long teamId, long tournamentId, long venueId, Domain.Enums.Enum.EventStatus eventStatus)
        {
            /* if (eventStatus == 0)
             {*/
            var result = await (from events in _context.Event.AsNoTracking()
                                join evenTeam in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus)
                               .Include(x => x.EventCollectionTeam.Team)
                               .Include(x => x.EventCollectionTeam.EventCollection.EventType)
                               .Include(x => x.EventCollectionTeam.EventCollection.Activity)
                               .Include(x => x.EventCollectionTeam.EventCollection.Activity.ActivityCategory)
                               .Include(x => x.EventCollectionTeam.EventCollection.Venue)
                               .Include(x => x.EventCollectionTeam.EventCollection.Venue.Branch).Where(x => x.EventCollectionTeam.EventCollection.Venue.Branch.UserInfoId == userId)
                               .Include(x => x.EventCollectionTeam.EventCollection.UserInfo.User)
                               .Include(x => x.Event.UserInfo.User)

                                on events.Id equals evenTeam.EventId

                               // where paginationFilter.sSearch != "" ? evenTeam.EventCollectionTeam.Team.Name.Contains(paginationFilter.sSearch) : paginationFilter.sSearch == ""// team Filter on events

                                // where eventStatus != 0 ? evenTeam.EventStatus.Name.ToLower() == eventStatus.ToString().ToLower() : events.Id != 0
                                join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                on evenTeam.EventCollectionTeamId equals eventTeamCollection.Id

                                join eventCollectionTbl in _context.EventCollection.AsNoTracking().Include(x => x.Activity).Include(x => x.UserInfo)
                                on eventTeamCollection.EventCollectionId equals eventCollectionTbl.Id

                                join venueTbl in _context.Venue.AsNoTracking()
                                on eventCollectionTbl.VenueId equals venueTbl.Id

                                join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                on eventCollectionTbl.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                where teamId != 0 ? evenTeam.EventCollectionTeam.TeamId == teamId : evenTeam.EventCollectionTeam.TeamId != 0


                                where tournamentId != 0 ?
                                tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                eventCollectionTbl.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : events.Id != 0

                                where venueId != 0 ? eventCollectionTbl.VenueId == venueId : events.Id != 0
                                where eventStatus != 0 ? evenTeam.EventStatus.Name.ToLower() == eventStatus.ToString().ToLower() : events.Id != 0

                                group evenTeam by evenTeam.EventId into g

                                select new BranchesVenuesResponseTemp
                                {
                                    Id = g.Key,
                                    VenueName = g.First().EventCollectionTeam.EventCollection.Venue.Name,
                                    venueId = g.First().EventCollectionTeam.EventCollection.Venue.Id,
                                    BranchName = g.First().EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                    StatskeeperName = g.First().EventCollectionTeam.EventCollection.UserInfo.Name,
                                    EventType = g.First().EventCollectionTeam.EventCollection.EventType.Name,
                                    Status = g.First().EventStatus.Name,
                                    CurrentActivity = g.First().EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name,
                                    StartTime = g.First().Event.EventDateTime,
                                    EndTime = g.First().Event.EventEndDateTime,
                                    EventDate = g.First().Event.EventDateTime,
                                    EventDuration = g.First().Event.EventEndDateTime.Subtract(g.First().Event.EventDateTime).TotalMinutes,
                                    /*   ActivityIcon = _context.Image.AsNoTracking().Where(x => x.TableId == g.First().EventCollectionTeam.EventCollection.Activity.Id && x.ConnectionEntity.Name.ToLower()
                                                      == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower()).Select(x => x.Path).FirstOrDefault(),
                                       StatkeeperImage = _context.Image.AsNoTracking().Include(x => x.ConnectionEntity).Include(x => x.ImageType)
                                                         .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower()
                                                         && x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                                                         && x.TableId == g.First().EventCollectionTeam.EventCollection.UserInfo.Id
                                                         ).Select(x => x.Path).FirstOrDefault(),*/
                                    Teams = g.Select(t => new TeamDetail
                                    {
                                        Name = t.EventCollectionTeam.Team.Name,
                                        Id = t.EventCollectionTeam.TeamId,
                                        //Points = g.First().Points,
                                    }).ToList()
                                }).OrderByDescending(x => x.Id).Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                            .Take(paginationFilter.PageSize)
                            .ToListAsync();
            return result;
            /* }*/
            /*else
            {
                var eventStatusId = eventStatus.ToString();
                var result = await (from events in _context.Event.AsNoTracking()
                                    join evenTeam in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Where(x => x.EventStatus.Name == eventStatusId)
                               .Include(x => x.EventCollectionTeam.Team)
                               .Include(x => x.EventCollectionTeam.EventCollection.EventType)
                               .Include(x => x.EventCollectionTeam.EventCollection.Activity)
                               .Include(x => x.EventCollectionTeam.EventCollection.Activity.ActivityCategory)
                               .Include(x => x.EventCollectionTeam.EventCollection.Venue)
                               .Include(x => x.EventCollectionTeam.EventCollection.Venue.Branch).Where(x => x.EventCollectionTeam.EventCollection.Venue.Branch.UserInfoId == userId)
                               .Include(x => x.EventCollectionTeam.EventCollection.UserInfo.User)

                               .Include(x => x.Event.UserInfo.User)
                                on events.Id equals evenTeam.EventId

                                    join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                    on evenTeam.EventCollectionTeamId equals eventTeamCollection.Id

                                    join eventCollectionTbl in _context.EventCollection.AsNoTracking().Include(x => x.Activity).Include(x => x.UserInfo)
                                    on eventTeamCollection.EventCollectionId equals eventCollectionTbl.Id

                                    join venueTbl in _context.Venue.AsNoTracking()
                                    on eventCollectionTbl.VenueId equals venueTbl.Id


                                    where teamId != 0 ? evenTeam.EventCollectionTeam.TeamId == teamId : evenTeam.EventCollectionTeam.TeamId != 0

                                    where tournamentId != 0 ?
                                    tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                    eventCollectionTbl.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : events.Id != 0

                                    where venueId != 0 ? eventCollectionTbl.VenueId == venueId : events.Id != 0
                                    where eventStatus != 0 ? evenTeam.EventStatus.Name.ToLower() == eventStatus.ToString().ToLower() : events.Id != 0

                                    group evenTeam by evenTeam.EventId into g

                                    select new BranchesVenuesResponse
                                    {
                                        Id = g.Key,
                                        VenueName = g.First().EventCollectionTeam.EventCollection.Venue.Name,
                                        venueId = g.First().EventCollectionTeam.EventCollection.Venue.Id,
                                        BranchName = g.First().EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                        StatskeeperName = g.First().EventCollectionTeam.EventCollection.UserInfo.Name,
                                        EventType = g.First().EventCollectionTeam.EventCollection.EventType.Name,
                                        Status = g.First().EventStatus.Name,
                                        CurrentActivity = g.First().EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name,
                                        StartTime = g.First().Event.EventDateTime,
                                        EndTime = g.First().Event.EventEndDateTime,
                                        EventDate = g.First().Event.EventDateTime,
                                        EventDuration = g.First().Event.EventEndDateTime.Subtract(g.First().Event.EventDateTime).TotalMinutes,
                                       *//* ActivityIcon = _context.Image.AsNoTracking().Where(x => x.TableId == g.First().EventCollectionTeam.EventCollection.Activity.Id && x.ConnectionEntity.Name.ToLower()
                                                       == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower()).Select(x => x.Path).FirstOrDefault(),
                                        StatkeeperImage = _context.Image.AsNoTracking().Include(x => x.ConnectionEntity).Include(x => x.ImageType)
                                                          .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower()
                                                          && x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                                                          && x.TableId == g.First().EventCollectionTeam.EventCollection.UserInfo.Id
                                                          ).Select(x => x.Path).FirstOrDefault(),*//*
                                        Teams = g.Select(t => new TeamDetail
                                        {
                                            Name = t.EventCollectionTeam.Team.Name,
                                            Id = t.EventCollectionTeam.TeamId,
                                            //Points = g.First().Points,
                                        }).ToList()
                                    }).OrderByDescending(x => x.Id).Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                .Take(paginationFilter.PageSize)
                                .ToListAsync();
                return result;
            }*/


        }


        public async Task<List<BranchesVenuesResponseTemp>> GetBranchesVenuesListByStatkeeper(BasicFilter paginationFilter, long userId, long teamId, long tournamentId, long venueId, Domain.Enums.Enum.EventStatus eventStatus)
        {
            /*if (eventStatus == 0)
            {*/
            var result = await (from events in _context.Event
                                join evenTeam in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus)
                               .Include(x => x.EventCollectionTeam.Team)
                               .Include(x => x.EventCollectionTeam.EventCollection.EventType)
                               .Include(x => x.EventCollectionTeam.EventCollection.Activity)
                               .Include(x => x.EventCollectionTeam.EventCollection.Venue)
                               .Include(x => x.EventCollectionTeam.EventCollection.Venue.Branch).Where(x => x.EventCollectionTeam.EventCollection.StatKeeperId == userId)
                               .Include(x => x.EventCollectionTeam.EventCollection.UserInfo.User)
                               .Include(x => x.EventStatus)
                               .Include(x => x.Event.UserInfo.User)
                                on events.Id equals evenTeam.EventId

                                where paginationFilter.sSearch != "" ? evenTeam.EventCollectionTeam.Team.Name.Contains(paginationFilter.sSearch) : paginationFilter.sSearch == ""// team Filter on events
                                where !evenTeam.EventCollectionTeam.EventCollection.IsDeleted
                                join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                on evenTeam.EventCollectionTeamId equals eventTeamCollection.Id

                                join eventCollectionTbl in _context.EventCollection.AsNoTracking().Include(x => x.Activity).Include(x => x.UserInfo)
                                on eventTeamCollection.EventCollectionId equals eventCollectionTbl.Id

                                join venueTbl in _context.Venue.AsNoTracking()
                                on eventCollectionTbl.VenueId equals venueTbl.Id

                                join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                on eventCollectionTbl.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                where teamId != 0 ? evenTeam.EventCollectionTeam.TeamId == teamId : evenTeam.EventCollectionTeam.TeamId != 0

                                where tournamentId != 0 ?
                                tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                eventCollectionTbl.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : events.Id != 0

                                where venueId != 0 ? eventCollectionTbl.VenueId == venueId : events.Id != 0
                                where eventStatus != 0 ? evenTeam.EventStatus.Name.ToLower() == eventStatus.ToString().ToLower() : events.Id != 0



                                group evenTeam by evenTeam.EventId into g

                                select new BranchesVenuesResponseTemp
                                {
                                    Id = g.Key,
                                    VenueName = g.First().EventCollectionTeam.EventCollection.Venue.Name,
                                    venueId = g.First().EventCollectionTeam.EventCollection.Venue.Id,
                                    BranchName = g.First().EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                    StatskeeperName = g.First().EventCollectionTeam.EventCollection.UserInfo.Name,
                                    EventType = g.First().EventCollectionTeam.EventCollection.EventType.Name,
                                    Status = g.First().EventStatus.Name,
                                    CurrentActivity = g.First().EventCollectionTeam.EventCollection.Activity.Name,
                                    /*Teams = g.Select(t => new TeamDetail
                                    {
                                        Name = t.EventCollectionTeam.Team.Name,
                                        Id = t.EventCollectionTeam.TeamId,
                                        //  Points = t.Points
                                    }).ToList()*/
                                }).Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                .Take(paginationFilter.PageSize)
                                .ToListAsync();

            return result;
            /* }*/
            /*  else
              {
                  var eventStatusName = eventStatus.ToString();
                  var result = await (from events in _context.Event
                                      join evenTeam in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Where(x => x.EventStatus.Name == eventStatusName)
                                     .Include(x => x.EventCollectionTeam.Team)
                                     .Include(x => x.EventCollectionTeam.EventCollection.EventType)
                                     .Include(x => x.EventCollectionTeam.EventCollection.Activity)
                                     .Include(x => x.EventCollectionTeam.EventCollection.Venue)
                                     .Include(x => x.EventCollectionTeam.EventCollection.Venue.Branch).Where(x => x.EventCollectionTeam.EventCollection.StatKeeperId == userId)
                                     .Include(x => x.EventCollectionTeam.EventCollection.UserInfo.User)
                                     .Include(x => x.EventStatus)
                                     .Include(x => x.Event.UserInfo.User)
                                      on events.Id equals evenTeam.EventId
                                      where !evenTeam.EventCollectionTeam.EventCollection.IsDeleted


                                      group evenTeam by evenTeam.EventId into g

                                      select new BranchesVenuesResponse
                                      {
                                          Id = g.Key,
                                          VenueName = g.First().EventCollectionTeam.EventCollection.Venue.Name,
                                          venueId = g.First().EventCollectionTeam.EventCollection.Venue.Id,
                                          BranchName = g.First().EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                          StatskeeperName = g.First().EventCollectionTeam.EventCollection.UserInfo.Name,
                                          EventType = g.First().EventCollectionTeam.EventCollection.EventType.Name,
                                          Status = g.First().EventStatus.Name,
                                          CurrentActivity = g.First().EventCollectionTeam.EventCollection.Activity.Name,
                                          Teams = g.Select(t => new TeamDetail
                                          {
                                              Name = t.EventCollectionTeam.Team.Name,
                                              Id = t.EventCollectionTeam.TeamId,
                                              //  Points = t.Points
                                          }).ToList()
                                      }).Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                      .Take(paginationFilter.PageSize)
                                      .ToListAsync();
                  return result;
              }*/

        }



        public async Task<int> GetBranchesVenuesListByStatkeeperCount(long userId, long teamId, long tournamentId, long venueId, Domain.Enums.Enum.EventStatus eventStatus, string search)
        {
            /* if (eventStatus == 0)

             {*/
            var result = await (from events in _context.Event
                                join evenTeam in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus)
                               .Include(x => x.EventCollectionTeam.Team)
                               .Include(x => x.EventCollectionTeam.EventCollection.EventType)
                               .Include(x => x.EventCollectionTeam.EventCollection.Activity)
                               .Include(x => x.EventCollectionTeam.EventCollection.Venue)
                               .Include(x => x.EventCollectionTeam.EventCollection.Venue.Branch).Where(x => x.EventCollectionTeam.EventCollection.StatKeeperId == userId)
                               .Include(x => x.EventCollectionTeam.EventCollection.UserInfo.User)
                               .Include(x => x.EventStatus)
                               .Include(x => x.Event.UserInfo.User)
                                on events.Id equals evenTeam.EventId


                                where !evenTeam.EventCollectionTeam.EventCollection.IsDeleted
                                join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                on evenTeam.EventCollectionTeamId equals eventTeamCollection.Id

                                where search != "" ? evenTeam.EventCollectionTeam.Team.Name.Contains(search) : search == ""// team Filter on events

                                join eventCollectionTbl in _context.EventCollection.AsNoTracking().Include(x => x.Activity).Include(x => x.UserInfo)
                                on eventTeamCollection.EventCollectionId equals eventCollectionTbl.Id

                                join venueTbl in _context.Venue.AsNoTracking()
                                on eventCollectionTbl.VenueId equals venueTbl.Id

                                join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                on eventCollectionTbl.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                where teamId != 0 ? evenTeam.EventCollectionTeam.TeamId == teamId : evenTeam.EventCollectionTeam.TeamId != 0

                                where tournamentId != 0 ?
                                tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                eventCollectionTbl.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : events.Id != 0

                                where venueId != 0 ? eventCollectionTbl.VenueId == venueId : events.Id != 0
                                where eventStatus != 0 ? evenTeam.EventStatus.Name.ToLower() == eventStatus.ToString().ToLower() : events.Id != 0



                                group evenTeam by evenTeam.EventId into g

                                select new BranchesVenuesResponse
                                {
                                    Id = g.Key,
                                    VenueName = g.First().EventCollectionTeam.EventCollection.Venue.Name,
                                    venueId = g.First().EventCollectionTeam.EventCollection.Venue.Id,
                                    BranchName = g.First().EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                    StatskeeperName = g.First().EventCollectionTeam.EventCollection.UserInfo.Name,
                                    EventType = g.First().EventCollectionTeam.EventCollection.EventType.Name,
                                    Status = g.First().EventStatus.Name,
                                    CurrentActivity = g.First().EventCollectionTeam.EventCollection.Activity.Name,
                                    Teams = g.Select(t => new TeamDetail
                                    {
                                        Name = t.EventCollectionTeam.Team.Name,
                                        Id = t.EventCollectionTeam.TeamId,
                                        //  Points = t.Points
                                    }).ToList()
                                }).CountAsync();

            return result;
        }

        public async Task<int> GetBranchesVenuesCount(long userId, long teamId, long tournamentId, long venueId, Domain.Enums.Enum.EventStatus eventStatus, string search)
        {
            var result = await (from events in _context.Event.AsNoTracking()
                                join evenTeam in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus)
                                .Include(x => x.EventCollectionTeam.Team)
                                .Include(x => x.EventCollectionTeam.EventCollection.EventType)
                                .Include(x => x.EventCollectionTeam.EventCollection.Activity)
                                .Include(x => x.EventCollectionTeam.EventCollection.Activity.ActivityCategory)
                                .Include(x => x.EventCollectionTeam.EventCollection.Venue)
                                .Include(x => x.EventCollectionTeam.EventCollection.Venue.Branch).Where(x => x.EventCollectionTeam.EventCollection.Venue.Branch.UserInfoId == userId)
                                .Include(x => x.EventCollectionTeam.EventCollection.UserInfo.User)
                                .Include(x => x.Event.UserInfo.User)

                                on events.Id equals evenTeam.EventId

                                where search != "" ? evenTeam.EventCollectionTeam.Team.Name.Contains(search) : search == ""// team Filter on events
                                join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                on evenTeam.EventCollectionTeamId equals eventTeamCollection.Id

                                join eventCollectionTbl in _context.EventCollection.AsNoTracking().Include(x => x.Activity).Include(x => x.UserInfo)
                                on eventTeamCollection.EventCollectionId equals eventCollectionTbl.Id

                                join venueTbl in _context.Venue.AsNoTracking()
                                on eventCollectionTbl.VenueId equals venueTbl.Id

                                join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                on eventCollectionTbl.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                where teamId != 0 ? evenTeam.EventCollectionTeam.TeamId == teamId : evenTeam.EventCollectionTeam.TeamId != 0

                                where tournamentId != 0 ?
                                tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                eventCollectionTbl.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : events.Id != 0

                                where venueId != 0 ? eventCollectionTbl.VenueId == venueId : events.Id != 0
                                where eventStatus != 0 ? evenTeam.EventStatus.Name.ToLower() == eventStatus.ToString().ToLower() : events.Id != 0

                                group evenTeam by evenTeam.EventId into g

                                select new BranchesVenuesResponse
                                {
                                    Id = g.Key,
                                    VenueName = g.First().EventCollectionTeam.EventCollection.Venue.Name,
                                    venueId = g.First().EventCollectionTeam.EventCollection.Venue.Id,
                                    BranchName = g.First().EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                    StatskeeperName = g.First().EventCollectionTeam.EventCollection.UserInfo.Name,
                                    EventType = g.First().EventCollectionTeam.EventCollection.EventType.Name,
                                    Status = g.First().EventStatus.Name,
                                    CurrentActivity = g.First().EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name,
                                    StartTime = g.First().Event.EventDateTime,
                                    EndTime = g.First().Event.EventEndDateTime,
                                    EventDate = g.First().Event.EventDateTime,
                                    EventDuration = g.First().Event.EventEndDateTime.Subtract(g.First().Event.EventDateTime).TotalMinutes,
                                    /*   ActivityIcon = _context.Image.AsNoTracking().Where(x => x.TableId == g.First().EventCollectionTeam.EventCollection.Activity.Id && x.ConnectionEntity.Name.ToLower()
                                                      == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower()).Select(x => x.Path).FirstOrDefault(),
                                       StatkeeperImage = _context.Image.AsNoTracking().Include(x => x.ConnectionEntity).Include(x => x.ImageType)
                                                         .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower()
                                                         && x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                                                         && x.TableId == g.First().EventCollectionTeam.EventCollection.UserInfo.Id
                                                         ).Select(x => x.Path).FirstOrDefault(),*/
                                    Teams = g.Select(t => new TeamDetail
                                    {
                                        Name = t.EventCollectionTeam.Team.Name,
                                        Id = t.EventCollectionTeam.TeamId,
                                        //Points = g.First().Points,
                                    }).ToList()
                                }).CountAsync();
            return result;
        }

        public async Task<List<BranchesVenuesResponse>> GetBranchesVenuesListByPlayer(BasicFilter paginationFilter, long userId, long teamId, long tournamentId,
            long venueId, Domain.Enums.Enum.EventStatus eventStatus, long activityCategoryId)
        {
            IQueryable<long> eventList;
            IQueryable<long> searchTeamEventList;

            searchTeamEventList = (from eventTbl in _context.Event.AsNoTracking()
                                   join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                   on eventTbl.Id equals eventTeamTbl.EventId
                                   join teamTbl in _context.Team.AsNoTracking()
                                   on eventTeamTbl.EventCollectionTeam.TeamId equals teamTbl.Id

                                   //join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                   //on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                   //where teamMemberTbl.PlayerId == userId

                                   where eventTeamTbl.EventCollectionTeam.Team.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                         || eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                         || eventTeamTbl.EventCollectionTeam.EventCollection.Activity.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                         || eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                         || eventTeamTbl.EventCollectionTeam.EventCollection.UserInfo.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())

                                   select eventTbl.Id
                                   ).AsQueryable();

            if (searchTeamEventList.Any())
            {
                {
                    if (teamId != 0)
                    {
                        eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                     join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                     on eventTbl.Id equals eventTeamTbl.EventId
                                     //join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                     //on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                     join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                     on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                     from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                         //where teamMemberTbl.PlayerId == userId
                                     where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0

                                     where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                     where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                     where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                     eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                     select eventTbl.Id
                                    ).AsQueryable();
                    }
                    else
                    {
                        eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                     join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                     on eventTbl.Id equals eventTeamTbl.EventId
                                     join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                     on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                     join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                     on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                     from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                     where teamMemberTbl.PlayerId == userId
                                     where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0
                                     where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                     where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                     where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                     eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                     select eventTbl.Id
                                     ).AsQueryable();
                    }

                    var queryResult = await (from eventTbl in eventList
                                             join eventTeamTbl in _context.EventTeam.Include(x => x.EventCollectionTeam.Team).AsNoTracking()
                                             on eventTbl equals eventTeamTbl.EventId
                                             join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                             on eventTeamTbl.EventCollectionTeamId equals eventTeamCollection.Id

                                             join userImageTbl in _context.Image.AsNoTracking()
                                             .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                                             .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                             on eventTeamCollection.EventCollection.StatKeeperId equals userImageTbl.TableId into newDbImage
                                             from UserImageFull in newDbImage.DefaultIfEmpty()

                                             join activityImageTbl in _context.Image.AsNoTracking()
                                             .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower())
                                             .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                             on eventTeamCollection.EventCollection.ActivityId equals activityImageTbl.TableId into newactivityImageTblImage
                                             from activityImageTblFull in newactivityImageTblImage.DefaultIfEmpty()
                                             select new
                                             {
                                                 eventTeamTbl = eventTeamTbl,
                                                 UserImagePath = UserImageFull != null ? UserImageFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                                 activityImagePath = activityImageTblFull != null ? activityImageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                                             }).GroupBy(x => x.eventTeamTbl.EventId)
                                             .Select(eventGroupCollection => new BranchesVenuesResponse
                                             {
                                                 Id = eventGroupCollection.Key,
                                                 VenueName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Name,
                                                 venueId = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Id,
                                                 BranchName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                                 StatskeeperName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.UserInfo.Name,
                                                 EventType = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name,
                                                 Status = eventGroupCollection.First().eventTeamTbl.EventStatus.Name,
                                                 CurrentActivity = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name,
                                                 StartTime = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                                 EndTime = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime,
                                                 EventDate = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                                 EventDuration = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime.Subtract(eventGroupCollection.First().eventTeamTbl.Event.EventDateTime).TotalMinutes,
                                                 ActivityIcon = eventGroupCollection.First().activityImagePath,
                                                 StatkeeperImage = eventGroupCollection.First().UserImagePath,
                                                 Teams = eventGroupCollection.Select(eventTeam => new TeamDetail
                                                 {
                                                     eventid = eventGroupCollection.Key,
                                                     Id = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Id,
                                                     Name = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Name,
                                                     Points = eventTeam.eventTeamTbl.Points,
                                                     Status = eventTeam.eventTeamTbl.EventStatus.Name,
                                                     Colour = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Colour,
                                                     Round = eventTeam.eventTeamTbl.Round
                                                 })
                                                 .ToList()

                                             })
                                             .OrderByDescending(x => x.Id).Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                             .Take(paginationFilter.PageSize)
                                             .ToListAsync();
                    return queryResult;
                }

            }
            // Filter for status 
            else
            {
                if (teamId != 0)
                {
                    eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                 join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                 on eventTbl.Id equals eventTeamTbl.EventId
                     
                                 join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                 on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                 from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                 where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0

                                 where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                 where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                 where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                 eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                 select eventTbl.Id
                                ).AsQueryable();
                }
                else
                {
                    searchTeamEventList = (from eventTbl in _context.Event.AsNoTracking()
                                           join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                           on eventTbl.Id equals eventTeamTbl.EventId
                                           join teamTbl in _context.Team.AsNoTracking()
                                           on eventTeamTbl.EventCollectionTeam.TeamId equals teamTbl.Id

                                           join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                           on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                           where teamMemberTbl.PlayerId == userId

                                         
                                           select eventTbl.Id
                                         ).AsQueryable();

                    eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                 join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                 on eventTbl.Id equals eventTeamTbl.EventId
                                 join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                 on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                 join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                 on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                 from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                 where teamMemberTbl.PlayerId == userId
                                 where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0
                           
                                 where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                 where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                 where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                 eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                 select eventTbl.Id
                                 ).AsQueryable();
                }

                var queryResult = await (from eventTbl in eventList
                                         join eventTeamTbl in _context.EventTeam.Include(x => x.EventCollectionTeam.Team).AsNoTracking()
                                         on eventTbl equals eventTeamTbl.EventId
                                         join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                         on eventTeamTbl.EventCollectionTeamId equals eventTeamCollection.Id

                                         join userImageTbl in _context.Image.AsNoTracking()
                                         .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                                         .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                         on eventTeamCollection.EventCollection.StatKeeperId equals userImageTbl.TableId into newDbImage
                                         from UserImageFull in newDbImage.DefaultIfEmpty()

                                         join activityImageTbl in _context.Image.AsNoTracking()
                                         .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower())
                                         .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                         on eventTeamCollection.EventCollection.ActivityId equals activityImageTbl.TableId into newactivityImageTblImage
                                         from activityImageTblFull in newactivityImageTblImage.DefaultIfEmpty()
                                         select new
                                         {
                                             eventTeamTbl = eventTeamTbl,
                                             UserImagePath = UserImageFull != null ? UserImageFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                             activityImagePath = activityImageTblFull != null ? activityImageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                                         }).GroupBy(x => x.eventTeamTbl.EventId)
                                         .Select(eventGroupCollection => new BranchesVenuesResponse
                                         {
                                             Id = eventGroupCollection.Key,
                                             VenueName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Name,
                                             venueId = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Id,
                                             BranchName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                             StatskeeperName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.UserInfo.Name,
                                             EventType = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name,
                                             Status = eventGroupCollection.First().eventTeamTbl.EventStatus.Name,
                                             CurrentActivity = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name,
                                             StartTime = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                             EndTime = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime,
                                             EventDate = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                             EventDuration = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime.Subtract(eventGroupCollection.First().eventTeamTbl.Event.EventDateTime).TotalMinutes,
                                             ActivityIcon = eventGroupCollection.First().activityImagePath,
                                             StatkeeperImage = eventGroupCollection.First().UserImagePath,
                                             Teams = eventGroupCollection.Select(eventTeam => new TeamDetail
                                             {
                                                 eventid = eventGroupCollection.Key,
                                                 Id = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Id,
                                                 Name = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Name,
                                                 Points = eventTeam.eventTeamTbl.Points,
                                                 Status = eventTeam.eventTeamTbl.EventStatus.Name,
                                                 Colour = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Colour,
                                                 Round = eventTeam.eventTeamTbl.Round
                                             }).ToList()
                                             }).Where(x => !string.IsNullOrEmpty(paginationFilter.sSearch) ?       
                                              x.Status.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()):
                                              x.Id != 0)
                                         .OrderByDescending(x => x.Id).Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                         .Take(paginationFilter.PageSize)
                                         .ToListAsync();
                return queryResult;
            }

        }


        public async Task<int> GetBranchesVenuesPlayerCount(BasicFilter paginationFilter, long userId, long tournamentId, long teamId, long venueId, Domain.Enums.Enum.EventStatus eventStatus
          , long activityCategoryId, string sSearch)
        {
            IQueryable<long> eventList;
            IQueryable<long> searchTeamEventList;
            searchTeamEventList = (from eventTbl in _context.Event.AsNoTracking()
                                   join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                   on eventTbl.Id equals eventTeamTbl.EventId
                                   join teamTbl in _context.Team.AsNoTracking()
                                   on eventTeamTbl.EventCollectionTeam.TeamId equals teamTbl.Id

                                   //join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                   //on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                   //where teamMemberTbl.PlayerId == userId

                                   where teamTbl.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                     || eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                     || eventTeamTbl.EventCollectionTeam.EventCollection.Activity.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                     || eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                     || eventTeamTbl.EventCollectionTeam.EventCollection.UserInfo.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())

                                   select eventTbl.Id
                                         ).AsQueryable();

            if (searchTeamEventList.Any())
            {


                {
                    if (teamId != 0)
                    {
                        eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                     join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                     on eventTbl.Id equals eventTeamTbl.EventId
                                     //join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                     //on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                     join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                     on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                     from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                         //where teamMemberTbl.PlayerId == userId
                                     where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0

                                     where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                     where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                     where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                     eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                     select eventTbl.Id
                                    ).AsQueryable();
                    }
                    else
                    {
                        eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                     join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                     on eventTbl.Id equals eventTeamTbl.EventId
                                     join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                     on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                     join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                     on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                     from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                     where teamMemberTbl.PlayerId == userId
                                     where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0
                                     where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                     where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                     where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                     eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                     select eventTbl.Id
                                     ).AsQueryable();
                    }

                    var queryResult = await (from eventTbl in eventList
                                             join eventTeamTbl in _context.EventTeam.Include(x => x.EventCollectionTeam.Team).AsNoTracking()
                                             on eventTbl equals eventTeamTbl.EventId
                                             join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                             on eventTeamTbl.EventCollectionTeamId equals eventTeamCollection.Id

                                             join userImageTbl in _context.Image.AsNoTracking()
                                             .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                                             .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                             on eventTeamCollection.EventCollection.StatKeeperId equals userImageTbl.TableId into newDbImage
                                             from UserImageFull in newDbImage.DefaultIfEmpty()

                                             join activityImageTbl in _context.Image.AsNoTracking()
                                             .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower())
                                             .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                             on eventTeamCollection.EventCollection.ActivityId equals activityImageTbl.TableId into newactivityImageTblImage
                                             from activityImageTblFull in newactivityImageTblImage.DefaultIfEmpty()
                                             select new
                                             {
                                                 eventTeamTbl = eventTeamTbl,
                                                 UserImagePath = UserImageFull != null ? UserImageFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                                 activityImagePath = activityImageTblFull != null ? activityImageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                                             }).GroupBy(x => x.eventTeamTbl.EventId)
                                             .Select(eventGroupCollection => new BranchesVenuesResponse
                                             {
                                                 Id = eventGroupCollection.Key,
                                                 VenueName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Name,
                                                 venueId = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Id,
                                                 BranchName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                                 StatskeeperName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.UserInfo.Name,
                                                 EventType = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name,
                                                 Status = eventGroupCollection.First().eventTeamTbl.EventStatus.Name,
                                                 CurrentActivity = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name,
                                                 StartTime = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                                 EndTime = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime,
                                                 EventDate = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                                 EventDuration = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime.Subtract(eventGroupCollection.First().eventTeamTbl.Event.EventDateTime).TotalMinutes,
                                                 ActivityIcon = eventGroupCollection.First().activityImagePath,
                                                 StatkeeperImage = eventGroupCollection.First().UserImagePath,
                                                 Teams = eventGroupCollection.Select(eventTeam => new TeamDetail
                                                 {
                                                     eventid = eventGroupCollection.Key,
                                                     Id = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Id,
                                                     Name = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Name,
                                                     Points = eventTeam.eventTeamTbl.Points,
                                                     Status = eventTeam.eventTeamTbl.EventStatus.Name,
                                                     Colour = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Colour,
                                                     Round = eventTeam.eventTeamTbl.Round
                                                 })
                                                 .ToList()

                                             })
                                             .OrderByDescending(x => x.Id).Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                             .Take(paginationFilter.PageSize).CountAsync();
                    return queryResult;
                }

            }
            else
            {
                if (teamId != 0)
                {
                    eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                 join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                 on eventTbl.Id equals eventTeamTbl.EventId

                                 join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                 on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                 from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                 where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0

                                 where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                 where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                 where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                 eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                 select eventTbl.Id
                                ).AsQueryable();
                }
                else
                {
                    searchTeamEventList = (from eventTbl in _context.Event.AsNoTracking()
                                           join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                           on eventTbl.Id equals eventTeamTbl.EventId
                                           join teamTbl in _context.Team.AsNoTracking()
                                           on eventTeamTbl.EventCollectionTeam.TeamId equals teamTbl.Id

                                           join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                           on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                           where teamMemberTbl.PlayerId == userId


                                           select eventTbl.Id
                                         ).AsQueryable();

                    eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                 join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                 on eventTbl.Id equals eventTeamTbl.EventId
                                 join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                 on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                 join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                 on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                 from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                 where teamMemberTbl.PlayerId == userId
                                 where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0

                                 where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                 where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                 where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                 eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                 select eventTbl.Id
                                 ).AsQueryable();
                }

                var queryResult = await (from eventTbl in eventList
                                         join eventTeamTbl in _context.EventTeam.Include(x => x.EventCollectionTeam.Team).AsNoTracking()
                                         on eventTbl equals eventTeamTbl.EventId
                                         join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                         on eventTeamTbl.EventCollectionTeamId equals eventTeamCollection.Id

                                         join userImageTbl in _context.Image.AsNoTracking()
                                         .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                                         .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                         on eventTeamCollection.EventCollection.StatKeeperId equals userImageTbl.TableId into newDbImage
                                         from UserImageFull in newDbImage.DefaultIfEmpty()

                                         join activityImageTbl in _context.Image.AsNoTracking()
                                         .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower())
                                         .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                         on eventTeamCollection.EventCollection.ActivityId equals activityImageTbl.TableId into newactivityImageTblImage
                                         from activityImageTblFull in newactivityImageTblImage.DefaultIfEmpty()
                                         select new
                                         {
                                             eventTeamTbl = eventTeamTbl,
                                             UserImagePath = UserImageFull != null ? UserImageFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                             activityImagePath = activityImageTblFull != null ? activityImageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                                         }).GroupBy(x => x.eventTeamTbl.EventId)
                                         .Select(eventGroupCollection => new BranchesVenuesResponse
                                         {
                                             Id = eventGroupCollection.Key,
                                             VenueName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Name,
                                             venueId = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Id,
                                             BranchName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                             StatskeeperName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.UserInfo.Name,
                                             EventType = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name,
                                             Status = eventGroupCollection.First().eventTeamTbl.EventStatus.Name,
                                             CurrentActivity = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name,
                                             StartTime = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                             EndTime = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime,
                                             EventDate = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                             EventDuration = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime.Subtract(eventGroupCollection.First().eventTeamTbl.Event.EventDateTime).TotalMinutes,
                                             ActivityIcon = eventGroupCollection.First().activityImagePath,
                                             StatkeeperImage = eventGroupCollection.First().UserImagePath,
                                             Teams = eventGroupCollection.Select(eventTeam => new TeamDetail
                                             {
                                                 eventid = eventGroupCollection.Key,
                                                 Id = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Id,
                                                 Name = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Name,
                                                 Points = eventTeam.eventTeamTbl.Points,
                                                 Status = eventTeam.eventTeamTbl.EventStatus.Name,
                                                 Colour = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Colour,
                                                 Round = eventTeam.eventTeamTbl.Round
                                             }).ToList()
                                         }).Where(x => !string.IsNullOrEmpty(paginationFilter.sSearch) ?
                                          x.Status.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) :
                                          x.Id != 0)
                                         .CountAsync();
                                         
                                      
                return queryResult;
            }

        }


        public async Task<List<TeamDetail>> GetBranchesVenuesListByTeam(List<long> eventsId)
        {
            var results = await (from events in _context.Event
                                 join evenTeam in _context.EventTeam
                                 on events.Id equals evenTeam.EventId

                                 where !evenTeam.EventCollectionTeam.EventCollection.IsDeleted

                                 join eventCollectionTeam in _context.EventCollectionTeam
                                 on evenTeam.EventCollectionTeamId equals eventCollectionTeam.Id

                                 join team in _context.Team
                                 on eventCollectionTeam.TeamId equals team.Id

                                 where eventsId.Contains(events.Id)
                                 select new TeamDetail
                                 {
                                     eventid = events.Id,
                                     Name = team.Name,
                                     Id = team.Id,
                                     Status = evenTeam.EventStatus.Name,
                                     Points = evenTeam.Points,
                                     Colour = team.Colour
                                 }).ToListAsync();
            return results;

        }

        public async Task<List<Icon>> GetBranchesVenuesListActivityIcons(List<long> activityId)
        {
            var Activity = Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower();
            var results = await (from image in _context.Image
                                 join activity in _context.Activity
                                 on image.TableId equals activity.Id

                                 join connectionEntity in _context.ConnectionEntity
                                 on image.ConnectionEntityId equals connectionEntity.Id


                                 where activityId.Contains(image.TableId)
                                 select new Icon
                                 {
                                     Id = activity.Id,
                                     Path = image.Path,
                                 }).ToListAsync();

            return results;
        }
        public async Task<List<Icon>> GetBranchesVenuesListStatkeeperIcons(List<long> statkeeperId)
        {
           
            var results = await (from image in _context.Image
                                 join userInfo in _context.UserInfo
                                 on image.TableId equals userInfo.Id

                                 join connectionEntity in _context.ConnectionEntity
                                 on image.ConnectionEntityId equals connectionEntity.Id


                                 where statkeeperId.Contains(image.TableId)
                                 select new Icon
                                 {
                                     Id = userInfo.Id,
                                     Path = image.Path,
                                 }).ToListAsync();

            return results;
        }


        public async Task<string> GetStatusByUserId(long userId, List<TeamDetail> Teams)
        {
            var teamIds = Teams.Select(t => t.Id).ToList();

            var eventStatus = await _context.EventTeam
                .Where(et => teamIds.Contains(et.EventCollectionTeam.TeamId))
                .Join(_context.TeamMember,
                    et => et.EventCollectionTeam.TeamId,
                    tm => tm.TeamId,
                    (et, tm) => new { EventTeam = et, TeamMember = tm })
                .Join(_context.EventStatus,
                    etm => etm.EventTeam.EventStatusId,
                    es => es.Id,
                    (etm, es) => new { EventTeam = etm.EventTeam, TeamMember = etm.TeamMember, EventStatus = es })
                .FirstOrDefaultAsync(etmes => etmes.TeamMember.PlayerId == userId);

            if (eventStatus != null)
            {
                return eventStatus.EventStatus.Name;
            }

            return string.Empty; // If no eventStatus is found

           

            
        }

        public async Task<List<BranchesVenuesResponse>> GetBranchesVenuesListByPlayerTemp(BasicFilter paginationFilter, long userId, long teamId, long tournamentId,
            long venueId, Domain.Enums.Enum.EventStatus eventStatus, long activityCategoryId)
        {
            IQueryable<long> eventList;
            IQueryable<long> searchTeamEventList;

            searchTeamEventList = (from eventTbl in _context.Event.AsNoTracking()
                                   join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                   on eventTbl.Id equals eventTeamTbl.EventId

                                   join teamTbl in _context.Team.AsNoTracking()
                                   on eventTeamTbl.EventCollectionTeam.TeamId equals teamTbl.Id


                                   where eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.UserInfoId == userId


                                   //join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                   //on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                   //where teamMemberTbl.PlayerId == userId

                                   where    eventTeamTbl.EventCollectionTeam.Team.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                         || eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                         || eventTeamTbl.EventCollectionTeam.EventCollection.Activity.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                         || eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())
                                         || eventTeamTbl.EventCollectionTeam.EventCollection.UserInfo.Name.ToLower().Contains(paginationFilter.sSearch.ToLower())

                                   select eventTbl.Id
                                   ).AsQueryable();

            if (searchTeamEventList.Any())
            {
                {
                    if (teamId != 0)
                    {
                        eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                     join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                     on eventTbl.Id equals eventTeamTbl.EventId
                                     //join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                     //on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                     join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                     on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                     from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                         //where teamMemberTbl.PlayerId == userId
                                     where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0

                                     where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                     where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                     where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                     eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                     select eventTbl.Id
                                    ).AsQueryable();
                    }
                    else
                    {
                        eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                     join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                     on eventTbl.Id equals eventTeamTbl.EventId

                                     join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                     on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId

                                     join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                     on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                     from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                     //where teamMemberTbl.PlayerId == userId
                                     where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0
                                     where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                     where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                     where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                     eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                     select eventTbl.Id
                                     ).AsQueryable();
                    }

                    var queryResult = await (from eventTbl in eventList
                                             join eventTeamTbl in _context.EventTeam.Include(x => x.EventCollectionTeam.Team).AsNoTracking()
                                             on eventTbl equals eventTeamTbl.EventId
                                             join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                             on eventTeamTbl.EventCollectionTeamId equals eventTeamCollection.Id

                                             join userImageTbl in _context.Image.AsNoTracking()
                                             .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                                             .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                             on eventTeamCollection.EventCollection.StatKeeperId equals userImageTbl.TableId into newDbImage
                                             from UserImageFull in newDbImage.DefaultIfEmpty()

                                             join activityImageTbl in _context.Image.AsNoTracking()
                                             .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower())
                                             .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                             on eventTeamCollection.EventCollection.ActivityId equals activityImageTbl.TableId into newactivityImageTblImage
                                             from activityImageTblFull in newactivityImageTblImage.DefaultIfEmpty()
                                             select new
                                             {
                                                 eventTeamTbl = eventTeamTbl,
                                                 UserImagePath = UserImageFull != null ? UserImageFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                                 activityImagePath = activityImageTblFull != null ? activityImageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                                             }).GroupBy(x => x.eventTeamTbl.EventId)
                                             .Select(eventGroupCollection => new BranchesVenuesResponse
                                             {
                                                 Id = eventGroupCollection.Key,
                                                 VenueName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Name,
                                                 venueId = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Id,
                                                 BranchName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                                 StatskeeperName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.UserInfo.Name,
                                                 EventType = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name,
                                                 Status = eventGroupCollection.First().eventTeamTbl.EventStatus.Name,
                                                 CurrentActivity = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name,
                                                 StartTime = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                                 EndTime = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime,
                                                 EventDate = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                                 EventDuration = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime.Subtract(eventGroupCollection.First().eventTeamTbl.Event.EventDateTime).TotalMinutes,
                                                 ActivityIcon = eventGroupCollection.First().activityImagePath,
                                                 StatkeeperImage = eventGroupCollection.First().UserImagePath,
                                                 Teams = eventGroupCollection.Select(eventTeam => new TeamDetail
                                                 {
                                                     eventid = eventGroupCollection.Key,
                                                     Id = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Id,
                                                     Name = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Name,
                                                     Points = eventTeam.eventTeamTbl.Points,
                                                     Status = eventTeam.eventTeamTbl.EventStatus.Name,
                                                     Colour = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Colour,
                                                     Round = eventTeam.eventTeamTbl.Round
                                                 })
                                                 .ToList()

                                             })
                                             .OrderByDescending(x => x.Id).Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                             .Take(paginationFilter.PageSize)
                                             .ToListAsync();
                    return queryResult;
                }

            }
            // Filter for status 
            else
            {
                if (teamId != 0)
                {
                    eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                 join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                 on eventTbl.Id equals eventTeamTbl.EventId

                                 join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                 on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                 from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                 where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0

                                 where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                 where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                 where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                 eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                 select eventTbl.Id
                                ).AsQueryable();
                }
                else
                {
                    searchTeamEventList = (from eventTbl in _context.Event.AsNoTracking()
                                           join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                           on eventTbl.Id equals eventTeamTbl.EventId
                                           join teamTbl in _context.Team.AsNoTracking()
                                           on eventTeamTbl.EventCollectionTeam.TeamId equals teamTbl.Id

                                           join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                           on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                           where teamMemberTbl.PlayerId == userId


                                           select eventTbl.Id
                                         ).AsQueryable();

                    eventList = (from eventTbl in _context.Event.AsNoTracking().Where(x => searchTeamEventList.Contains(x.Id))
                                 join eventTeamTbl in _context.EventTeam.AsNoTracking()
                                 on eventTbl.Id equals eventTeamTbl.EventId
                                 join teamMemberTbl in _context.TeamMember.AsNoTracking()
                                 on eventTeamTbl.EventCollectionTeam.TeamId equals teamMemberTbl.TeamId
                                 join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                                 on eventTeamTbl.EventCollectionTeam.EventCollection.Id equals tournamentEventCollectionTbl.EventCollectionId into newRightOuter
                                 from tournamentEventCollectionFullTbl in newRightOuter.DefaultIfEmpty()

                                 where teamMemberTbl.PlayerId == userId
                                 where teamId != 0 ? eventTeamTbl.EventCollectionTeam.TeamId == teamId : eventTbl.Id != 0

                                 where activityCategoryId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategoryId == activityCategoryId : eventTbl.Id != 0
                                 where venueId != 0 ? eventTeamTbl.EventCollectionTeam.EventCollection.VenueId == venueId : eventTbl.Id != 0
                                 where tournamentId != 0 ? tournamentEventCollectionFullTbl.TournamentId == tournamentId &&
                                 eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower() : eventTbl.Id != 0

                                 select eventTbl.Id
                                 ).AsQueryable();
                }

                var queryResult = await (from eventTbl in eventList
                                         join eventTeamTbl in _context.EventTeam.Include(x => x.EventCollectionTeam.Team).AsNoTracking()
                                         on eventTbl equals eventTeamTbl.EventId
                                         join eventTeamCollection in _context.EventCollectionTeam.AsNoTracking()
                                         on eventTeamTbl.EventCollectionTeamId equals eventTeamCollection.Id

                                         join userImageTbl in _context.Image.AsNoTracking()
                                         .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                                         .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                         on eventTeamCollection.EventCollection.StatKeeperId equals userImageTbl.TableId into newDbImage
                                         from UserImageFull in newDbImage.DefaultIfEmpty()

                                         join activityImageTbl in _context.Image.AsNoTracking()
                                         .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower())
                                         .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                         on eventTeamCollection.EventCollection.ActivityId equals activityImageTbl.TableId into newactivityImageTblImage
                                         from activityImageTblFull in newactivityImageTblImage.DefaultIfEmpty()
                                         select new
                                         {
                                             eventTeamTbl = eventTeamTbl,
                                             UserImagePath = UserImageFull != null ? UserImageFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                             activityImagePath = activityImageTblFull != null ? activityImageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                                         }).GroupBy(x => x.eventTeamTbl.EventId)
                                         .Select(eventGroupCollection => new BranchesVenuesResponse
                                         {
                                             Id = eventGroupCollection.Key,
                                             VenueName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Name,
                                             venueId = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Id,
                                             BranchName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Venue.Branch.Name,
                                             StatskeeperName = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.UserInfo.Name,
                                             EventType = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.EventType.Name,
                                             Status = eventGroupCollection.First().eventTeamTbl.EventStatus.Name,
                                             CurrentActivity = eventGroupCollection.First().eventTeamTbl.EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name,
                                             StartTime = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                             EndTime = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime,
                                             EventDate = eventGroupCollection.First().eventTeamTbl.Event.EventDateTime,
                                             EventDuration = eventGroupCollection.First().eventTeamTbl.Event.EventEndDateTime.Subtract(eventGroupCollection.First().eventTeamTbl.Event.EventDateTime).TotalMinutes,
                                             ActivityIcon = eventGroupCollection.First().activityImagePath,
                                             StatkeeperImage = eventGroupCollection.First().UserImagePath,
                                             Teams = eventGroupCollection.Select(eventTeam => new TeamDetail
                                             {
                                                 eventid = eventGroupCollection.Key,
                                                 Id = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Id,
                                                 Name = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Name,
                                                 Points = eventTeam.eventTeamTbl.Points,
                                                 Status = eventTeam.eventTeamTbl.EventStatus.Name,
                                                 Colour = eventTeam.eventTeamTbl.EventCollectionTeam.Team.Colour,
                                                 Round = eventTeam.eventTeamTbl.Round
                                             }).ToList()
                                         }).Where(x => !string.IsNullOrEmpty(paginationFilter.sSearch) ?
                                          x.Status.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) :
                                          x.Id != 0)
                                         .OrderByDescending(x => x.Id).Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                         .Take(paginationFilter.PageSize)
                                         .ToListAsync();
                return queryResult;
            }

        }


    }

}
