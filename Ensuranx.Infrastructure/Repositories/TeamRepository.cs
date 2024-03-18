using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Team;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Common.Constant;
using Ensuranx.Domain.Entities;
using Ensuranx.Domain.Enums;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeamMemberDetails>> GetTeamsWithMembersListByPlayerId(string role, long userId, BasicFilter basicFilter)
        {
            return await (from team in _context.Team.AsNoTracking()
                          join eventCollectionTeam in _context.EventCollectionTeam.AsNoTracking()
                          on team.Id equals eventCollectionTeam.TeamId into newFullTeamCollection
                          from teamCollectionOuter in newFullTeamCollection.DefaultIfEmpty()
                          join eventCollection in _context.EventCollection.AsNoTracking()
                          on teamCollectionOuter.EventCollectionId equals eventCollection.Id into newFullEventCollection
                          join teamMember in _context.TeamMember.AsNoTracking()
                          .Include(x => x.UserInfo)
                          .Include(x => x.UserInfo.User)
                          .Include(x => x.Team)
                          .Include(x => x.TeamMemberRole)
                          .Include(x => x.Team.TeamType)
                          on team.Id equals teamMember.TeamId

                          where teamMember.PlayerId == userId
                          //where !string.IsNullOrEmpty(basicFilter.sSearch) ? 
                          //      (
                          //       team.Name.Contains(basicFilter.sSearch.Trim().ToLower()) ||
                          //       team.TeamType.Name.Contains(basicFilter.sSearch.Trim().ToLower())  ||
                          //       teamMember.UserInfo.Name.Contains(basicFilter.sSearch.Trim().ToLower())
                          //      )
                          //      : team.Id != 0
                          where team.IsDeleted != true && team.IsActive != false
                          group teamMember by teamMember.TeamId into g

                          select new TeamMemberDetails
                          {
                              Id = g.Key,
                              Name = g.First().Team.Name,
                              Type = g.First().Team.TeamType.Name,
                              Colour = g.First().Team.Colour,
                              LastPlayed = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.Team.Id == g.First().Team.Id).OrderByDescending(x => x.EventId).Select(d => d.Event.EventDateTime.ToShortDateString()).FirstOrDefault(),
                              DateFormed = g.First().Team.CreatedDateTime.ToShortDateString(),
                              Win = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()).Count(),
                              Loss = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower()).Count(),
                              Draw = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower()).Count(),
                              ActivePlayers = _context.TeamMember.AsNoTracking().Where(tm => tm.TeamId == g.First().Team.Id && tm.IsDeleted != true && tm.IsActive != false && tm.IsAccepted != false).Count(),
                              Matches = _context.EventTeam.AsNoTracking()
                                        .Where(et => et.EventCollectionTeam.Team.Id == g.First().Team.Id &&
                                        (et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower() &&
                                        et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() &&
                                        et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InActive.ToString().ToLower()))
                                        .Select(d => d.Event.Id).Count(),
                              RecentMatchActivity = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name).FirstOrDefault(),
                              RecentMatchStatkeeper = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.UserInfo.Name).FirstOrDefault(),
                              RecentMatchBranchName = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.Venue.Branch.Name).FirstOrDefault(),
                              RecentMatchVenueName = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.Venue.Name).FirstOrDefault(),
                              RecentMatchStatkeeperImage = (from eventTeam in _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                          .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.StatKeeperId)
                                                            join imageStatkeeper in _context.Image.AsNoTracking().Include(t => t.ImageType).Include(x => x.ConnectionEntity)
                                                            .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                                             x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                                             on eventTeam.Value equals imageStatkeeper.TableId
                                                            select imageStatkeeper.Path).FirstOrDefault() != null ? (from eventTeam in _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                           .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.StatKeeperId)
                                                                                                                     join imageStatkeeper in _context.Image.AsNoTracking().Include(t => t.ImageType).Include(x => x.ConnectionEntity)
                                                                                                                     .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                                                                                                     x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                                                                                                     on eventTeam.Value equals imageStatkeeper.TableId
                                                                                                                     select imageStatkeeper.Path).FirstOrDefault() : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                          }).Where(x => !string.IsNullOrEmpty(basicFilter.sSearch) ?
                                         (x.Name.ToLower().Contains(basicFilter.sSearch.Trim().ToLower())                  ||
                                          x.Type.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())                  ||
                                          x.RecentMatchStatkeeper.ToLower().Contains(basicFilter.sSearch.ToLower().Trim()) ||
                                          x.RecentMatchActivity.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())   ||
                                          x.RecentMatchBranchName.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())) :
                                          x.Id != 0)
                            .OrderByDescending(x => x.Id).Skip((basicFilter.PageNumber - 1) * basicFilter.PageSize)
                            .Take(basicFilter.PageSize).ToListAsync();
        }

        public async Task<List<GetRecentTeam>> GetRecentTeamsByPlayerIdAsync(long userId, BasicFilter basicFilter)
        {
            return await (from team in _context.Team.AsNoTracking().Include(x => x.TeamType)
                          join eventCollectionTeam in _context.EventCollectionTeam.AsNoTracking()
                          on team.Id equals eventCollectionTeam.TeamId

                          join eventCollection in _context.EventCollection.AsNoTracking().Include(x => x.UserInfo).Include(x => x.Activity.ActivityCategory)
                          on eventCollectionTeam.EventCollectionId equals eventCollection.Id

                          join teamMember in _context.TeamMember.AsNoTracking()
                          .Include(x => x.UserInfo.User)
                          .Include(x => x.Team)
                          .Include(x => x.TeamMemberRole)
                          .Include(x => x.Team.TeamType)
                          on team.Id equals teamMember.TeamId

                          join eventTeamTbl in _context.EventTeam.AsNoTracking()
                          on eventCollectionTeam.Id equals eventTeamTbl.EventCollectionTeamId

                          join venueTbl in _context.Venue.AsNoTracking().Include(x => x.Branch)
                          on eventCollection.VenueId equals venueTbl.Id

                          join imageTbl in _context.Image.AsNoTracking()
                          .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                      x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                          on eventCollection.StatKeeperId equals imageTbl.TableId into newFullImage
                          from imageTblFull in newFullImage.DefaultIfEmpty()

                          where team.IsDeleted != true && team.IsActive != false
                          where teamMember.PlayerId == userId

                          select new GetRecentTeam
                          {
                              Id = team.Id,
                              Name = team.Name,
                              Colour = team.Colour,
                              Type = team.TeamType.Name,
                              ActivePlayers = _context.TeamMember.AsNoTracking().Where(tm => tm.TeamId == team.Id).Count(),
                              RecentMatchActivity = eventCollection.Activity.ActivityCategory.Name,
                              RecentMatchBranchName = venueTbl.Branch.Name,
                              RecentMatchVenueName = venueTbl.Name,
                              RecentMatchStatkeeper = eventCollection.UserInfo.Name,
                              RecentMatchStatkeeperImage = imageTblFull != null ? imageTblFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                          }).Where(x => !string.IsNullOrEmpty(basicFilter.sSearch) ?
                                         (x.Name.ToLower().Contains(basicFilter.sSearch.Trim().ToLower())                   ||
                                          x.Type.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())                   ||
                                          x.RecentMatchStatkeeper.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())  ||
                                          x.RecentMatchActivity.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())    ||
                                          x.RecentMatchBranchName.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())) :
                                          x.Id != 0)
                          .Distinct().OrderByDescending(x => x.Id).Skip((basicFilter.PageNumber - 1) * basicFilter.PageSize)
                            .Take(basicFilter.PageSize).ToListAsync();
        }

        public async Task<int> GetRecentTeamsCountByPlayerIdAsync(long userId)
        {
            return await (from team in _context.Team.AsNoTracking().Include(x => x.TeamType)
                          join eventCollectionTeam in _context.EventCollectionTeam.AsNoTracking()
                          on team.Id equals eventCollectionTeam.TeamId

                          join eventCollection in _context.EventCollection.AsNoTracking().Include(x => x.UserInfo).Include(x => x.Activity.ActivityCategory)
                          on eventCollectionTeam.EventCollectionId equals eventCollection.Id

                          join teamMember in _context.TeamMember.AsNoTracking()
                          .Include(x => x.UserInfo.User)
                          .Include(x => x.Team)
                          .Include(x => x.TeamMemberRole)
                          .Include(x => x.Team.TeamType)
                          on team.Id equals teamMember.TeamId

                          join eventTeamTbl in _context.EventTeam.AsNoTracking()
                          on eventCollectionTeam.Id equals eventTeamTbl.EventCollectionTeamId

                          join venueTbl in _context.Venue.AsNoTracking().Include(x => x.Branch)
                          on eventCollection.VenueId equals venueTbl.Id

                          where teamMember.PlayerId == userId
                          where team.IsDeleted != true && team.IsActive != false

                          select new GetRecentTeam
                          {
                              Id = team.Id,
                          }).Distinct().CountAsync();
        }

        public async Task<DateTime> GetLastPlayedDateTimeByTeamId(long teamId)
        {
            return await (from teamTbl in _context.Team.AsNoTracking()
                          join eventTeam in _context.EventTeam.AsNoTracking().Include(e => e.Event)
                          on teamTbl.Id equals eventTeam.EventCollectionTeam.TeamId
                          where teamTbl.Id == teamId
                          orderby eventTeam.Id descending
                          select eventTeam.Event.EventDateTime).FirstOrDefaultAsync();
        }

        public async Task<GetTeamDetails> GetTeamsDetailByTeamId(long userId, long teamId)
        {
            return await (from team in _context.Team.AsNoTracking()
                          join eventCollectionTeam in _context.EventCollectionTeam.AsNoTracking()
                          on team.Id equals eventCollectionTeam.TeamId into newFullTeamCollection
                          from teamCollectionOuter in newFullTeamCollection.DefaultIfEmpty()
                          join eventCollection in _context.EventCollection.AsNoTracking()
                          on teamCollectionOuter.EventCollectionId equals eventCollection.Id into newFullEventCollection
                          join teamMember in _context.TeamMember
                          .Include(x => x.UserInfo.User)
                          .Include(x => x.Team)
                          .Include(x => x.TeamMemberRole)
                          .Include(x => x.Team.TeamType)
                          on team.Id equals teamMember.TeamId
                          join teamActivityTbl in _context.TeamActivity.Include(x => x.Activity.ActivityCategory).AsNoTracking()
                          on team.Id equals teamActivityTbl.TeamId into newFullRight
                          from teamActivityFullTbl in newFullRight.DefaultIfEmpty()

                          where team.Id == teamId

                          select new GetTeamDetails
                          {
                              TeamId = team.Id,
                              TeamName = team.Name,
                              TeamActivity = teamActivityFullTbl != null ? teamActivityFullTbl.Activity.ActivityCategory.Id : teamCollectionOuter != null ? teamCollectionOuter.EventCollection.Activity.ActivityCategoryId : 0,
                              TeamActivityName = teamActivityFullTbl != null ? teamActivityFullTbl.Activity.ActivityCategory.Name : teamCollectionOuter != null ? teamCollectionOuter.EventCollection.Activity.ActivityCategory.Name : "",
                              Colour = team.Colour,
                              TeamType = team.TeamType.Name,
                              LastPlayed = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.Team.Id == team.Id).OrderByDescending(x => x.EventId).Select(d => d.Event.EventDateTime).FirstOrDefault(),
                              DateFormed = team.CreatedDateTime,
                              Won = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()).Count(),
                              Loss = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower()).Count(),
                              Tie = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower()).Count(),
                              TeamMembersCount = _context.TeamMember.AsNoTracking().Where(tm => tm.TeamId == team.Id && tm.IsDeleted != true && tm.IsActive != false && tm.IsAccepted != false).Count(),
                              Match = _context.EventTeam.AsNoTracking()
                                        .Where(et => et.EventCollectionTeam.Team.Id == team.Id &&
                                        (et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower() &&
                                        et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() &&
                                        et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InActive.ToString().ToLower()))
                                        .Select(d => d.Event.Id).Count(),
                              IsCaptain = _context.TeamMember.AsNoTracking()
                                        .Any(x => x.TeamId == team.Id && x.PlayerId == userId && x.IsDeleted != true && x.IsActive != false && x.IsAccepted == true
                                        && x.TeamMemberRole.Name.ToLower() != Domain.Enums.Enum.TeamMemberRole.Member.ToString().ToLower())
                          }).FirstOrDefaultAsync();
        }

        public async Task<List<long>> GetTeamsIdListByPlayerId(long userId)
        {
            return await (from userInfoTbl in _context.UserInfo.AsNoTracking()
                          join teamMemberTbl in _context.TeamMember.AsNoTracking()
                          on userInfoTbl.Id equals teamMemberTbl.PlayerId
                          join team in _context.Team
                          on teamMemberTbl.TeamId equals team.Id

                          
                          where userInfoTbl.Id == userId
                          select teamMemberTbl.TeamId).ToListAsync();
        }

        public async Task<List<MemberDetail>> GetPlayersDetailsByTeamId(long userId, long teamId, string sSearch, bool isCaptain)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking().Include(x => x.TeamMemberRole)
                               join userInfoTbl in _context.UserInfo.AsNoTracking()
                               on teamMemberTbl.PlayerId equals userInfoTbl.Id

                               join userProfileImage in _context.Image.AsNoTracking()
                               .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                               on userInfoTbl.Id equals userProfileImage.TableId into newUserProfileImage
                               from userFullProfileImage in newUserProfileImage.DefaultIfEmpty()

                               join userCoverImage in _context.Image.AsNoTracking()
                               .Where(x => x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Cover.ToString().ToLower())
                               on userInfoTbl.Id equals userCoverImage.TableId into newUserCoverImage
                               from userFullCoverImage in newUserCoverImage.DefaultIfEmpty()

                               join followTbl in _context.Follow.AsNoTracking().Where(x => x.FollowerId == userId)
                               on userInfoTbl.Id equals followTbl.FolloweeId into newFullFollowee
                               from ifThisUserFollowTheseUser in newFullFollowee.DefaultIfEmpty()

                               join followTbl in _context.Follow.AsNoTracking().Where(x => x.FolloweeId == userId)
                               on userInfoTbl.Id equals followTbl.FollowerId into newFullFolloweeExt
                               from ifTheUserIsFollowedByTheseUser in newFullFolloweeExt.DefaultIfEmpty()

                               where teamMemberTbl.TeamId == teamId && teamMemberTbl.IsDeleted != true && teamMemberTbl.IsActive != false && teamMemberTbl.IsAccepted != false 
                               
                               where userFullProfileImage != null ? userFullProfileImage.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : teamMemberTbl.Id != 0
                               where userFullCoverImage != null ? userFullCoverImage.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : teamMemberTbl.Id != 0
                               where !string.IsNullOrEmpty(sSearch) ? userInfoTbl.Name.ToLower().Contains(sSearch.ToLower()) : teamMemberTbl.Id != 0
                               where isCaptain != true ? teamMemberTbl.TeamMemberRole.Name.ToLower() == Domain.Enums.Enum.TeamMemberRole.Member.ToString().ToLower() : teamMemberTbl.Id != 0

                               select new MemberDetail
                               {
                                   Id = userInfoTbl.Id,
                                   Name = userInfoTbl.Name,
                                   RoleName = teamId != 0 ? teamMemberTbl.TeamMemberRole.Name : "",
                                   ProfileImage = userFullProfileImage != null ? userFullProfileImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                   CoverImage = userFullCoverImage != null ? userFullCoverImage.Path : Constants.APIErrorMessages.USER_DEFAULT_COVER_IMAGE,
                                   Follow = ifThisUserFollowTheseUser != null ? ifThisUserFollowTheseUser.Status == 1 ? Follows.Following : Follows.RequestPending : Follows.RequestToFollow
                                   //ifTheUserIsFollowedByTheseUser != null ? ifTheUserIsFollowedByTheseUser.Status == 1 ? Follows.Following : Follows.RequestPending  : Follows.RequestToFollow
                               }).Distinct().ToListAsync();
            return query;
        }

        public async Task<List<EventTeamMemberStatResult>> GetPlayersStatsByTeamId(long userId, long teamId)
        {
            var query = await (from teamTbl in _context.Team.AsNoTracking()
                               join teamMemberTbl in _context.TeamMember.AsNoTracking()
                               on teamTbl.Id equals teamMemberTbl.TeamId
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking()
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId
                               join eventTeamMemberStatResultTbl in _context.EventTeamMemberStatResult.AsNoTracking().Include(x => x.TeamMember).ThenInclude(x => x.Team)
                               on teamMemberTbl.Id equals eventTeamMemberStatResultTbl.TeamMemberId 
                               /*into newEventTeamMemberStatResultTbl
                               from fullEventTeamMemberStatResultTbl in newEventTeamMemberStatResultTbl.DefaultIfEmpty()*/

                               where teamTbl.Id == teamId
                               select eventTeamMemberStatResultTbl

                               ).Distinct().ToListAsync();

            return query;
        }


        public async Task<List<TeamMemberDetails>> GetTeamsWithMembersListByBusinessId(long userId, BasicFilter basicFilter)
        {
            return await (from team in _context.Team
                          join eventCollectionTeam in _context.EventCollectionTeam.Where(x => x.EventCollection.Venue.Branch.Business.BusinessOwnerId == userId)
                          on team.Id equals eventCollectionTeam.TeamId

                          /*join eventCollection in _context.EventCollection.Include(x=>x.Venue.Branch)
                          on eventCollectionTeam.EventCollection equals eventCollection.Id
 */

                          join teamMember in _context.TeamMember
                         .Include(x => x.UserInfo.User)
                         .Include(x => x.Team)
                         .Include(x => x.TeamMemberRole)
                         .Include(x => x.Team.TeamType)
                          on team.Id equals teamMember.TeamId

                          group teamMember by teamMember.TeamId into g

                          select new TeamMemberDetails
                          {
                              Id = g.Key,
                              Name = g.First().Team.Name,
                              Colour = g.First().Team.Colour,
                              Type = g.First().Team.TeamType.Name,
                              LastPlayed = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.Team.Id == g.First().Team.Id).OrderByDescending(x => x.EventId).Select(d => d.Event.EventDateTime.ToShortDateString()).FirstOrDefault(),
                              DateFormed = g.First().Team.CreatedDateTime.ToShortDateString(),
                              Win = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()).Count(),
                              Loss = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower()).Count(),
                              Draw = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower()).Count(),
                              MemberDetailList = g.Distinct().Select(t => new MemberDetail
                              {
                                  Name = t.UserInfo.User.UserName,
                                  Id = t.UserInfo.Id,
                                  RoleName = t.TeamMemberRole.Name,
                                  Joined = t.IsAcceptedDateTime.ToShortDateString(),
                                  Left = t.LastModifiedDateTime.ToShortDateString(),
                                  ProfileImage = _context.Image.AsNoTracking().Include(t => t.ImageType).Include(x => x.ConnectionEntity)
                                  .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                  x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() && x.TableId == t.UserInfo.Id)
                                  .Select(p => p.Path).FirstOrDefault()
                              }).ToList(),
                              ActivePlayers = _context.TeamMember.AsNoTracking().Where(tm => tm.TeamId == g.First().Team.Id).Count(),
                              Matches = _context.EventTeam.AsNoTracking()
                                        .Where(et => et.EventCollectionTeam.Team.Id == g.First().Team.Id &&
                                        (et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower() &&
                                        et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() &&
                                        et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InActive.ToString().ToLower()))
                                        .Select(d => d.Event.Id).Count(),
                              RecentMatchActivity = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.Activity.Name).FirstOrDefault(),
                              RecentMatchStatkeeper = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.UserInfo.Name).FirstOrDefault(),
                              RecentMatchBranchName = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.Venue.Branch.Name).FirstOrDefault(),
                              RecentMatchVenueName = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.Venue.Name).FirstOrDefault(),
                              RecentMatchStatkeeperImage = (from eventTeam in _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                          .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.StatKeeperId)
                                                            join imageStatkeeper in _context.Image.AsNoTracking().Include(t => t.ImageType).Include(x => x.ConnectionEntity)
                                                            .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                                             x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                                            on eventTeam.Value equals imageStatkeeper.TableId
                                                            select imageStatkeeper.Path).FirstOrDefault(),
                          }).Skip((basicFilter.PageNumber - 1) * basicFilter.PageSize)
                            .Take(basicFilter.PageSize).ToListAsync();
        }
        public async Task<int> GetTeamsWithMembersCount(long userId)
        {
            return await (from team in _context.Team
                          join eventCollectionTeam in _context.EventCollectionTeam.Where(x => x.EventCollection.Venue.Branch.Business.BusinessOwnerId == userId)
                          on team.Id equals eventCollectionTeam.TeamId

                          /*join eventCollection in _context.EventCollection.Include(x=>x.Venue.Branch)
                          on eventCollectionTeam.EventCollection equals eventCollection.Id
 */

                          join teamMember in _context.TeamMember
                         .Include(x => x.UserInfo.User)
                         .Include(x => x.Team)
                         .Include(x => x.TeamMemberRole)
                         .Include(x => x.Team.TeamType)
                          on team.Id equals teamMember.TeamId

                          group teamMember by teamMember.TeamId into g

                          select new TeamMemberDetails
                          {
                              Id = g.Key,
                          }).CountAsync();
        }
        public async Task<int> GetTeamsWithMembersCountPlayer(long userId, string sSearch)
        {
            return await (from team in _context.Team.AsNoTracking()
                          join eventCollectionTeam in _context.EventCollectionTeam.AsNoTracking()
                          on team.Id equals eventCollectionTeam.TeamId into newFullTeamCollection
                          from teamCollectionOuter in newFullTeamCollection.DefaultIfEmpty()
                          join eventCollection in _context.EventCollection.AsNoTracking()
                          on teamCollectionOuter.EventCollectionId equals eventCollection.Id into newFullEventCollection
                          join teamMember in _context.TeamMember.AsNoTracking()
                          .Include(x => x.UserInfo.User)
                          .Include(x => x.Team)
                          .Include(x => x.TeamMemberRole)
                          .Include(x => x.Team.TeamType)
                          on team.Id equals teamMember.TeamId

                          where teamMember.PlayerId == userId
                          // where !string.IsNullOrEmpty(basicFilter.sSearch) ? team.Name.ToLower().Contains(basicFilter.sSearch.Trim().ToLower()) : team.Id != 0
                          where team.IsDeleted != true && team.IsActive != false
                          group teamMember by teamMember.TeamId into g

                          select new TeamMemberDetails
                          {
                              Id = g.Key,
                              Name = g.First().Team.Name,
                              Type = g.First().Team.TeamType.Name,
                              Colour = g.First().Team.Colour,
                              LastPlayed = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.Team.Id == g.First().Team.Id).OrderByDescending(x => x.EventId).Select(d => d.Event.EventDateTime.ToShortDateString()).FirstOrDefault(),
                              DateFormed = g.First().Team.CreatedDateTime.ToShortDateString(),
                              Win = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()).Count(),
                              Loss = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower()).Count(),
                              Draw = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id).Include(x => x.EventStatus)
                              .Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower()).Count(),
                              ActivePlayers = _context.TeamMember.AsNoTracking().Where(tm => tm.TeamId == g.First().Team.Id && tm.IsDeleted != true && tm.IsActive != false && tm.IsAccepted != false).Count(),
                              Matches = _context.EventTeam.AsNoTracking()
                                        .Where(et => et.EventCollectionTeam.Team.Id == g.First().Team.Id &&
                                        (et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower() &&
                                        et.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower()))
                                        .Select(d => d.Event.Id).Count(),
                              RecentMatchActivity = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.Activity.ActivityCategory.Name).FirstOrDefault(),
                              RecentMatchStatkeeper = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.UserInfo.Name).FirstOrDefault(),
                              RecentMatchBranchName = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.Venue.Branch.Name).FirstOrDefault(),
                              RecentMatchVenueName = _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                    .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.Venue.Name).FirstOrDefault(),
                              RecentMatchStatkeeperImage = (from eventTeam in _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                          .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.StatKeeperId)
                                                            join imageStatkeeper in _context.Image.AsNoTracking().Include(t => t.ImageType).Include(x => x.ConnectionEntity)
                                                            .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                                             x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                                             on eventTeam.Value equals imageStatkeeper.TableId
                                                            select imageStatkeeper.Path).FirstOrDefault() != null ? (from eventTeam in _context.EventTeam.AsNoTracking().Where(et => et.EventCollectionTeam.TeamId == g.First().Team.Id)
                                                           .OrderByDescending(x => x.Id).Select(x => x.EventCollectionTeam.EventCollection.StatKeeperId)
                                                                                                                     join imageStatkeeper in _context.Image.AsNoTracking().Include(t => t.ImageType).Include(x => x.ConnectionEntity)
                                                                                                                     .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                                                                                                     x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                                                                                                     on eventTeam.Value equals imageStatkeeper.TableId
                                                                                                                     select imageStatkeeper.Path).FirstOrDefault() : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                          }).Where(x => !string.IsNullOrEmpty(sSearch) ?
                                         (x.Name.ToLower().Contains(sSearch.Trim().ToLower())                  ||
                                          x.Type.ToLower().Contains(sSearch.ToLower().Trim())                  ||
                                          x.RecentMatchStatkeeper.ToLower().Contains(sSearch.ToLower().Trim()) ||
                                          x.RecentMatchActivity.ToLower().Contains(sSearch.ToLower().Trim())   ||
                                          x.RecentMatchBranchName.ToLower().Contains(sSearch.ToLower().Trim())) :
                                          x.Id != 0)
                          .CountAsync();
        }

        public async Task<bool> CheckTeamNameTaken(string name)
        {
            var query = await _context.Team.AsNoTracking().Where(x => x.Name.ToLower() == name.ToLower()).Select(x => x.Name).FirstOrDefaultAsync();
            return string.IsNullOrEmpty(query) ? false : true;
        }

        public async Task<List<GetAllInvites>> GetAllInvitesToJoinTeamList(long teamId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking().Include(u => u.UserInfo)
                               .Where(x => x.TeamId == teamId && x.IsAccepted != true && x.IsActive != false && x.IsDeleted != true)
                               join userImageTbl in _context.Image.AsNoTracking()
                               .Include(x => x.ImageType)
                               .Include(x => x.ConnectionEntity)
                               on teamMemberTbl.PlayerId equals userImageTbl.TableId into newfullrange
                               from teamMemberFull in newfullrange.DefaultIfEmpty()
                               where teamMemberFull != null && teamMemberFull.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                               teamMemberFull.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()

                               select new GetAllInvites
                               {
                                   PlayerId = teamMemberTbl.PlayerId.Value,
                                   PlayerName = teamMemberTbl.UserInfo.Name,
                                   PlayerImage = teamMemberFull != null ? teamMemberFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE
                               }).ToListAsync();
            return query;
        }

        public async Task<List<TeamGenericObj>> GetTeamForDropdownAsync(long userId, Domain.Enums.Enum.TeamType teamType, long tournamentId)
        {
            var tournamentActivityCheck = _context.Tournament.AsNoTracking().Where(x => x.Id == tournamentId).Select(x => x.ActivityId).FirstOrDefault();

            var query = await (from teamTbl in _context.Team.AsNoTracking()
                               join teamMemberTbl in _context.TeamMember.AsNoTracking()
                               on teamTbl.Id equals teamMemberTbl.TeamId

                               join teamActivityTbl in _context.TeamActivity.AsNoTracking()
                               on teamTbl.Id equals teamActivityTbl.TeamId into fullJoinActivity
                               from teamActivityFull in fullJoinActivity.DefaultIfEmpty()

                               where teamTbl.IsActive != false && teamTbl.IsDeleted != true
                               where teamMemberTbl.PlayerId == userId && teamMemberTbl.IsAccepted != false
                               where teamType != Domain.Enums.Enum.TeamType.All ? teamTbl.TeamType.Name.ToLower() == teamType.ToString().ToLower() : teamTbl.Id != 0
                               where tournamentId != 0 && teamActivityFull != null ? tournamentActivityCheck == teamActivityFull.ActivityId : teamTbl.Id != 0


                               select new TeamGenericObj
                               {
                                   Id = teamTbl.Id,
                                   Name = teamTbl.Name,
                                   Colour = teamTbl.Colour
                               }).Distinct().ToListAsync();
            return query;
        }

        public async Task<Domain.Entities.Team> GetTeamById(long teamId)
        {
            return await _context.Team.AsNoTracking().Where(x => x.Id == teamId).Include(x => x.TeamType).FirstOrDefaultAsync();
        }

        public async Task<Domain.Entities.Team> DeleteTeamById(Domain.Entities.Team team)
        {
            team.IsDeleted = true;
            team.IsActive = false;
            _context.Team.Update(team);
            await _context.SaveChangesAsync();

            return team;
        }

        public async Task<int> GetMatchesWonByTeamId(long teamId, int lastDays)
        {
            return await _context.EventTeam.AsNoTracking()
                        .Where(x => x.EventCollectionTeam.TeamId == teamId && x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower())
                        .Where(x => lastDays != 0 ? x.Event.EventDateTime > DateTime.UtcNow.AddDays(-lastDays) : x.Id != 0)
                        .CountAsync();
        }

        public async Task<int> GetMatchesLoseByTeamId(long teamId, int lastDays)
        {
            return await _context.EventTeam.AsNoTracking()
                        .Where(x => x.EventCollectionTeam.TeamId == teamId && x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower())
                        .Where(x => lastDays != 0 ? x.Event.EventDateTime > DateTime.UtcNow.AddDays(-lastDays) : x.Id != 0)
                        .CountAsync();
        }

        public async Task<int> GetMatchesTieByTeamId(long teamId, int lastDays)
        {
            return await _context.EventTeam.AsNoTracking()
                        .Where(x => x.EventCollectionTeam.TeamId == teamId && x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower())
                        .Where(x => lastDays != 0 ? x.Event.EventDateTime > DateTime.UtcNow.AddDays(-lastDays) : x.Id != 0)
                        .CountAsync();
        }

        public async Task<int> GetTeamCountInEventCollection(long eventCollectionId)
        {
            return await _context.EventCollectionTeam.AsNoTracking().Where(x => x.EventCollectionId == eventCollectionId).CountAsync();
        }

        public async Task<List<GetEventDetails>> GetTeamMatchesByTeamId(long teamId, int lastDays)
        {
            var query = await (from teamTbl in _context.Team.AsNoTracking()
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(e => e.Event).Include(s => s.EventStatus)
                               on teamTbl.Id equals eventTeamTbl.EventCollectionTeam.TeamId

                               join venueTbl in _context.Venue.AsNoTracking()
                               on eventTeamTbl.EventCollectionTeam.EventCollection.VenueId equals venueTbl.Id

                               join branchTbl in _context.Branch.AsNoTracking()
                               on venueTbl.BranchId equals branchTbl.Id

                               join activityTbl in _context.Activity.AsNoTracking().Include(x => x.ActivityCategory)
                               on eventTeamTbl.EventCollectionTeam.EventCollection.ActivityId equals activityTbl.Id

                               join userInfoTbl in _context.UserInfo.AsNoTracking()
                               on eventTeamTbl.EventCollectionTeam.EventCollection.StatKeeperId equals userInfoTbl.Id

                               join imageTbl in _context.Image.AsNoTracking()
                               .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                               on userInfoTbl.Id equals imageTbl.TableId into newImageTbl
                               from imageTblRight in newImageTbl.DefaultIfEmpty()

                               join imageActivityTbl in _context.Image.AsNoTracking()
                               .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower())
                               on activityTbl.Id equals imageActivityTbl.TableId into newImageActivityTbl
                               from imageActivityTblRight in newImageActivityTbl.DefaultIfEmpty()

                               where imageTblRight != null ? imageTblRight.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() : teamTbl.Id != 0
                               where imageActivityTblRight != null ? imageActivityTblRight.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() : teamTbl.Id != 0
                               where teamTbl.Id == teamId
                               where lastDays != 0 ? eventTeamTbl.Event.EventDateTime > DateTime.UtcNow.AddDays(-lastDays) : teamTbl.Id != 0
                               select new GetEventDetails
                               {
                                   EventId = eventTeamTbl.Event.Id,
                                   ActivityIcon = imageActivityTblRight != null ? imageActivityTblRight.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                   StatkeeperImage = imageTblRight != null ? imageTblRight.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                   ActivityName = activityTbl.ActivityCategory.Name,
                                   BranchName = branchTbl.Name,
                                   StatkeeperName = userInfoTbl.Name,
                                   VenueName = venueTbl.Name,
                                   EventDateTime = eventTeamTbl.Event.EventDateTime,
                                   Status = eventTeamTbl.EventStatus.Name
                               }).ToListAsync();
            return query;
        }

        public async Task<List<GetAllTournament>> GetTeamTournamentByTeamId(long teamId, int lastDays)
        {
            var query = await (from tournamentTbl in _context.Tournament.AsNoTracking().Include(a => a.Activity).ThenInclude(x => x.ActivityCategory)
                               join tournamentTeamTbl in _context.TournamentTeam.AsNoTracking()
                               on tournamentTbl.Id equals tournamentTeamTbl.TournamentId

                               join tournamentVenueTbl in _context.TournamentVenue.AsNoTracking().Include(b => b.Venue.Branch)
                               on tournamentTbl.Id equals tournamentVenueTbl.TournamentId

                               join userInfoTbl in _context.UserInfo.AsNoTracking().Include(u => u.User)
                               on tournamentTbl.CreatedBy equals userInfoTbl.User.Email

                               join imageTbl in _context.Image.AsNoTracking()
                               on userInfoTbl.Id equals imageTbl.TableId into newFullImage
                               from imageFullTbl in newFullImage.DefaultIfEmpty()

                               join imageActivityTbl in _context.Image.AsNoTracking()
                               .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower())
                               on tournamentTbl.ActivityId equals imageActivityTbl.TableId into newImageActivityTbl
                               from imageActivityTblRight in newImageActivityTbl.DefaultIfEmpty()

                               where tournamentTeamTbl.TeamId == teamId && tournamentTeamTbl.IsAccepted != false
                               where imageFullTbl != null ? imageFullTbl.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower()
                               && imageFullTbl.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() : tournamentTbl.Id != 0
                               where imageActivityTblRight != null ? imageActivityTblRight.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() : tournamentTbl.Id != 0
                               where lastDays != 0 ? tournamentTbl.StartDateTime > DateTime.UtcNow.AddDays(-lastDays) : tournamentTbl.Id != 0

                               select new GetAllTournament
                               {
                                   Id = tournamentTbl.Id,
                                   Name = tournamentTbl.Name,
                                   TeamCapacity = tournamentTbl.TeamCapacity,
                                   BranchName = tournamentVenueTbl.Venue.Branch.Name,
                                   ActivityName = tournamentTbl.Activity.ActivityCategory.Name,
                                   StartDateTime = tournamentTbl.StartDateTime,
                                   EndDateTime = tournamentTbl.EndDateTime,
                                   CreatedBy = userInfoTbl.Name,
                                   CreatedByImage = imageFullTbl != null ? imageFullTbl.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                   ActivityIcon = imageActivityTblRight != null ? imageActivityTblRight.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                   Status = DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) == 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Active.ToString() : DateTime.Compare(tournamentTbl.StartDateTime, DateTime.UtcNow) < 0 ?
                                   Domain.Enums.Enum.TournamentStatus.Completed.ToString() :
                                   Domain.Enums.Enum.TournamentStatus.Upcoming.ToString(),
                               }).ToListAsync();
            return query;
        }

        public async Task<string> GetRole(long roleId, long userId)
        {
            var results = await (from userRole in _context.UserRoles
                                 join user in _context.Users
                                 on userRole.UserId equals user.Id

                                 join userInfo in _context.UserInfo
                                 on user.Id equals userInfo.UserId

                                 join role in _context.Roles
                                 on userRole.RoleId equals role.Id

                                 where role.Id == roleId

                                 select role.Name).FirstOrDefaultAsync();


            return results;

        }
        public async Task<List<GetTeamDetailsByEventCollectionRes>> GetTeamsByEventCollection(long eventCollectionId,long userId)
        {
            var results = await (from eventCollection in _context.EventCollection
                                 join eventCollectionTeam in _context.EventCollectionTeam
                                 on eventCollection.Id equals eventCollectionTeam.EventCollectionId

                                 join eventTeam in _context.EventTeam.Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Active.ToString().ToLower())
                                 on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId into newFullEventTeamInProgress
                                 from eventTeamFullRight in newFullEventTeamInProgress.DefaultIfEmpty()

                                 join eventTeamTbl in _context.EventTeam.Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower())
                                 on eventCollectionTeam.Id equals eventTeamTbl.EventCollectionTeamId into newEventTeamTbl
                                 from eventTeamFullTbl in newEventTeamTbl.DefaultIfEmpty()

                                 join team in _context.Team
                                 on eventCollectionTeam.TeamId equals team.Id

                                 where team.IsDeleted != true && team.IsActive != false
                                 where eventCollection.Id == eventCollectionId
                                 where eventTeamFullTbl != null ? eventTeamFullTbl.EventCollectionTeam.TeamId != team.Id : team.Id != 0

                                 orderby eventCollectionTeam.Sequence ascending

                                 select new GetTeamDetailsByEventCollectionRes
                                 {
                                     teamId = team.Id,
                                     teamName = team.Name,
                                     Colour = team.Colour,
                                     IsFilled = _context.TeamMember.AsNoTracking().Where(x => x.TeamId == team.Id && x.IsAccepted != false && x.IsBan != true && x.IsActive != false && x.IsDeleted != true && x.IsSubstituteOrDisqualify != true).Count() ==
                                     eventCollection.Activity.MaxPlayerPerTeam ? true : false,
                                     IsJoined = _context.TeamMember.AsNoTracking().Any(x => x.TeamId == team.Id && x.IsAccepted != false && x.IsBan != true && x.IsActive != false && x.IsDeleted != true && x.PlayerId == userId),
                                     WinCount = _context.EventTeam.AsNoTracking().Where(x => x.EventCollectionTeam.TeamId == team.Id && x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()).Count(),
                                     InProgress = eventTeamFullRight != null && eventTeamFullRight.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() ? true : false
                                 }
                ).ToListAsync();
            return results;
        }

        public async Task<List<GetTeamDetailsByEventCollectionRes>> GetTeamsByEventId(long eventId, long userId)
        {
            var results = await (from eventCollection in _context.EventCollection.AsNoTracking()
                                 join eventCollectionTeam in _context.EventCollectionTeam
                                 on eventCollection.Id equals eventCollectionTeam.EventCollectionId
                                 
                                 join eventTeamTbl in _context.EventTeam.AsNoTracking().Where(x => x.EventId == eventId).Include(x => x.EventStatus)
                                 on eventCollectionTeam.Id equals eventTeamTbl.EventCollectionTeamId

                                 join team in _context.Team.AsNoTracking()
                                 on eventCollectionTeam.TeamId equals team.Id

                                 where team.IsDeleted != true && team.IsActive != false

                                 orderby eventCollectionTeam.Sequence ascending

                                 select new GetTeamDetailsByEventCollectionRes
                                 {
                                     teamId = team.Id,
                                     teamName = team.Name,
                                     Colour = team.Colour,
                                     IsFilled = _context.TeamMember.AsNoTracking().Where(x => x.TeamId == team.Id && x.IsAccepted != false && x.IsBan != true && x.IsActive != false && x.IsDeleted != true && x.IsSubstituteOrDisqualify != true && x.IsJoinedCollection != false).Count() ==
                                     eventCollection.Activity.MinPlayerPerTeam ? true : false,
                                     IsJoined = _context.TeamMember.AsNoTracking().Any(x => x.TeamId == team.Id && x.IsAccepted != false && x.IsBan != true && x.IsActive != false && x.IsDeleted != true && x.PlayerId == userId),
                                     WinCount = _context.EventTeam.AsNoTracking().Where(x => x.EventCollectionTeam.TeamId == team.Id && x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()).Count(),
                                     InProgress = eventTeamTbl.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() ? true : false
                                 }
                ).ToListAsync();
            return results;
        }

        public List<long> GetAllEvents(long userId, Domain.Enums.Enum.EventStatus eventStatus)
        {
            var events = (from eventss in _context.Event
                          join eventTeam in _context.EventTeam
                          on eventss.Id equals eventTeam.EventId

                          join eventTeamcollection in _context.EventCollectionTeam
                          on eventTeam.EventCollectionTeamId equals eventTeamcollection.Id

                          join team in _context.Team
                          on eventTeamcollection.TeamId equals team.Id

                          join teamMember in _context.TeamMember
                          on team.Id equals teamMember.TeamId
            where teamMember.PlayerId == userId
            where eventStatus != 0 ? eventTeam.EventStatus.Name.ToLower() == eventStatus.ToString().ToLower() : eventss.Id != 0
            select eventss.Id

                                ).ToList();
            return events;
        }

        public async Task<List<PlayerList>> GetPlayersbyTeamId(List<long> teamId)
        {
            var results = await (from teamMember in _context.TeamMember.Include(x => x.UserInfo)
                                 join team in _context.Team
                                 on teamMember.TeamId equals team.Id

                                 where teamId.Contains(teamMember.TeamId) && teamMember.IsActive != false && teamMember.IsDeleted != true 
                                 && teamMember.IsSubstituteOrDisqualify != true && teamMember.IsJoinedCollection != false
                                 select new PlayerList
                                 {
                                     teamId = team.Id,  
                                     playerId = teamMember.PlayerId,
                                     playerName = teamMember.UserInfo.Name,
                                     role = teamMember.TeamMemberRole.Name,
                                     sequence = teamMember.UserInfo.JerseyNo
                                 }).ToListAsync();
                                 
            return results;
        }


        public List<TeamDetail> GetAllteamsOnEvent(List<long> eventId)
        {
            var results = (from eventss in _context.Event
                           join eventTeam in _context.EventTeam
                           on eventss.Id equals eventTeam.EventId

                           join eventTeamcollection in _context.EventCollectionTeam
                           on eventTeam.EventCollectionTeamId equals eventTeamcollection.Id

                           join team in _context.Team
                           on eventTeamcollection.TeamId equals team.Id

                           where eventId.Contains(eventss.Id)
                           select new TeamDetail
                           {
                               Id = team.Id,
                               Name = team.Name
                           }).ToList();

            return results;

        }

        public async Task<List<GenericObj>> GetTeamsByEventCollectionId(long eventCollectionId, long userId)
        {
            return await (from evenCollectionTeam in _context.EventCollectionTeam.AsNoTracking()
                          join teamTbl in _context.Team.AsNoTracking()
                          on evenCollectionTeam.TeamId equals teamTbl.Id

                          where evenCollectionTeam.EventCollectionId == eventCollectionId

                          select new GenericObj
                          {
                              Id = teamTbl.Id,
                              Name = teamTbl.Name
                          }).ToListAsync();
        }
    }
}

