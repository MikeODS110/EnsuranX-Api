using ErrorOr;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Venue;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Application.Response.Venue;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;
using Event = Ensuranx.Domain.Entities.Event;

namespace Ensuranx.Infrastructure.Repositories
{
    public class VenueRepository : IVenueRepository
    {
        private readonly ApplicationDbContext _context;
        private IUnitOfWork<long> _iunitOfWork;
        public VenueRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<VenueResponse>> GetAllVenuesAsyncByStatkeeper(long userId, List<long> branchId)
        {
            var results = await (from venue in _context.Venue.AsNoTracking()
                                 join venuesStatskeeper in _context.VenueStatkeeper.AsNoTracking()
                                 on venue.Id equals venuesStatskeeper.VenueId
                                 where venuesStatskeeper.UserInfoId == userId
                                 where branchId.Any() ? branchId.Contains(venue.BranchId.Value) : venue.Id != 0

                                 select new VenueResponse
                                 {
                                     Id = venue.Id,
                                     Name = venue.Name,
                                 }).ToListAsync();

            return results;
        }
        public async Task<int> GetAllVenuesCountByStatKeeper(long userId, List<long> branchId)
        {
            var results = await (from venue in _context.Venue.AsNoTracking()
                                 join venuesStatskeeper in _context.VenueStatkeeper.AsNoTracking()
                                 on venue.Id equals venuesStatskeeper.VenueId
                                 where venuesStatskeeper.UserInfoId == userId
                                 where branchId.Any() ? branchId.Contains(venue.BranchId.Value) : venue.Id != 0

                                 select new VenueResponse
                                 {
                                     Id = venue.Id,
                                     Name = venue.Name,
                                 }).CountAsync();

            return results;
        }
        public async Task<List<VenueResponse>> GetAllVenuesAsyncByBusiness(long userId, List<long> branchId,long statkeeperId, long roleId)
        {
            var results = await (from venue in _context.Venue.AsNoTracking()
                                 join branch in _context.Branch.AsNoTracking()
                                 on venue.BranchId equals branch.Id
                                 where branch.UserInfoId == userId
                                 where branchId.Any() ? branchId.Contains(branch.Id) : venue.Id != 0
                                 join venueStatkeeper in _context.VenueStatkeeper.Where(x => statkeeperId != 0 ? x.UserInfoId == statkeeperId : x.Id != 0)
                                 on venue.Id equals venueStatkeeper.VenueId

                                 group venueStatkeeper by venueStatkeeper.VenueId into newVenueGrouped

                                 select new VenueResponse
                                 {
                                     Id = newVenueGrouped.Key,
                                     Name = newVenueGrouped.First().Venue.Name,
                                 }).ToListAsync();

            return results;
        }
        public async Task<int> GetAllVenuesCountByBusiness(long userId, List<long> branchId)
        {
            var results = await (from venue in _context.Venue
                                 join branch in _context.Branch
                                 on venue.BranchId equals branch.Id
                                 where branch.UserInfoId == userId

                                 where branchId.Any() ? branchId.Contains(branch.Id) : venue.Id != 0
                                 select new VenueResponse
                                 {
                                     Id = venue.Id,
                                     Name = venue.Name,
                                 }).CountAsync();

            return results;
        }
        public async Task<List<Venue>> GetVenuesByIdListAsync(List<long> venueIdList)
        {
            return await _context.Venue.AsNoTracking().Where(x => venueIdList.Contains(x.Id)).ToListAsync();
        }

        public async Task<List<GetVenueBrnach>> GetVenuesByIdUserIdAsync(long userId)
        {
            return await (from VenuesStatskeeper in _context.VenueStatkeeper.Where(x => x.UserInfoId == userId)
                          .Include(x => x.UserInfo.User)
                          join venue in _context.Venue
                          on VenuesStatskeeper.VenueId equals venue.Id

                          select new GetVenueBrnach
                          { 
                                
                                //BranchId=venue.Branch.Id,
                                VenueId= VenuesStatskeeper.VenueId,
                                VenueName= VenuesStatskeeper.Venue.Name
                          }).ToListAsync();

        }
        public async Task<List<GenericObj>> GetBranchesByIdUserIdAsync(long userId)
        {
            return await (from VenuesStatskeeper in _context.VenueStatkeeper.Where(x => x.UserInfoId == userId)
                          .Include(x => x.UserInfo.User)
                          join venue in _context.Venue
                          on VenuesStatskeeper.VenueId equals venue.Id

                          select new GenericObj
                          {

                              Id = venue.Branch.Id,
                              Name = venue.Branch.Name
                              
                          }).Distinct().ToListAsync();

        }


        public async Task<List<GetVenuesEventWithoutTeams>> GetVenuesEventWithoutTeamsBusiness(BasicFilter basicFilter, long userId)
        {
            List<GetVenuesEventWithoutTeams> VenuesEventListWithoutTeams = new List<GetVenuesEventWithoutTeams>();

            var results = await (from venue in _context.Venue
                                 join branch in _context.Branch
                                 on venue.BranchId equals branch.Id

                                 join userInfo in _context.UserInfo
                                 on branch.UserInfoId equals userInfo.Id

                                 where branch.UserInfoId == userId
                                 where basicFilter.sSearch != "" ? venue.Name.Contains(basicFilter.sSearch) : basicFilter.sSearch == ""

                                 join eventCollection in _context.EventCollection
                                 on venue.Id equals eventCollection.VenueId

                                 join eventCollectionTeam in _context.EventCollectionTeam
                                 on eventCollection.Id equals eventCollectionTeam.EventCollectionId

                                 join user in _context.UserInfo.Include(x => x.User)
                                 on eventCollection.StatKeeperId equals user.Id

                                 join activity in _context.Activity.Include(x => x.ActivityCategory)
                                 on eventCollection.ActivityId equals activity.Id

                                 join eventTeam in _context.EventTeam
                                 on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                 join events in _context.Event
                                 on eventTeam.EventId equals events.Id

                                 join eventStatus in _context.EventStatus
                                 on eventTeam.EventStatusId equals eventStatus.Id


                                 join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                 on user.Id equals imageTbl.TableId into newImageOuter
                                 from Image in newImageOuter.DefaultIfEmpty()

                                 where Image != null ? Image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() &&
                                 Image.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : venue.Id != 0

                                 join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                 on activity.Id equals imageTbl.TableId into newActivityImageOuter
                                 from ImageActivity in newActivityImageOuter.DefaultIfEmpty()


                                 select new GetVenuesEventWithoutTeams
                                 {
                                     Id = events.Id,
                                     VenueName = venue.Name,
                                     venueId = venue.Id,
                                     BranchName = branch.Name,
                                     CreatedDate = eventCollection.CreatedDateTime,
                                     StatskeeperName = user.Name,
                                     StatskeeperImage = Image != null ? Image.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     ActivityIcon = ImageActivity != null ? ImageActivity.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     StatskeeperContact = user.User.Email,
                                     Status = eventStatus.Name,
                                     CurrentActivity = activity.ActivityCategory.Name,
                                 }).Distinct().Skip((basicFilter.PageNumber - 1) * basicFilter.PageSize)
                            .Take(basicFilter.PageSize).ToListAsync();

            return results;
        }

        public async Task<int> GetVenuesEventWithoutTeamsBusinessCount(BasicFilter paginationFilter, long userId)
        {
            List<GetVenuesEventWithoutTeams> VenuesEventListWithoutTeams = new List<GetVenuesEventWithoutTeams>();

            var results = await (from venue in _context.Venue


                                 join branch in _context.Branch
                                 on venue.BranchId equals branch.Id

                                 join userInfo in _context.UserInfo
                                 on branch.UserInfoId equals userInfo.Id

                                 where branch.UserInfoId == userId
                                 where paginationFilter.sSearch != "" ? venue.Name.Contains(paginationFilter.sSearch) : paginationFilter.sSearch == ""

                                 join eventCollection in _context.EventCollection
                                 on venue.Id equals eventCollection.VenueId


                                 join eventCollectionTeam in _context.EventCollectionTeam
                                 on eventCollection.Id equals eventCollectionTeam.EventCollectionId



                                 join user in _context.UserInfo.Include(x => x.User)
                                 on eventCollection.StatKeeperId equals user.Id

                                 join activity in _context.Activity.Include(x => x.ActivityCategory)
                                 on eventCollection.ActivityId equals activity.Id

                                 join eventTeam in _context.EventTeam
                                 on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                 join events in _context.Event
                                 on eventTeam.EventId equals events.Id




                                 join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                 on user.Id equals imageTbl.TableId into newImageOuter
                                 from Image in newImageOuter.DefaultIfEmpty()



                                 where Image != null ? Image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() &&
                                 Image.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : venue.Id != 0

                                 join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                 on activity.Id equals imageTbl.TableId into newActivityImageOuter
                                 from ImageActivity in newActivityImageOuter.DefaultIfEmpty()


                                 select new GetVenuesEventWithoutTeams
                                 {
                                     Id = events.Id,
                                     VenueName = venue.Name,
                                     venueId = venue.Id,
                                     BranchName = branch.Name,
                                     CreatedDate = eventCollection.CreatedDateTime,
                                     StatskeeperName = user.Name,
                                     StatskeeperImage = Image != null ? Image.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     ActivityIcon = ImageActivity != null ? ImageActivity.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     StatskeeperContact = user.User.Email,

                                     CurrentActivity = activity.ActivityCategory.Name,
                                 }).Distinct().CountAsync();

            return results;
        }
        public async Task<List<GetVenuesEventWithoutTeams>> GetVenuesEventWithoutTeamsPlayer(BasicFilter basicFilter, long userId)
        {
            List<GetVenuesEventWithoutTeams> VenuesEventListWithoutTeams = new List<GetVenuesEventWithoutTeams>();


            var results = await (from venue in _context.Venue

                                 join branch in _context.Branch
                                 on venue.BranchId equals branch.Id

                                 /*  join userInfo in _context.UserInfo
                                    on branch.UserInfoId equals userInfo.Id*/



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

                                 join user in _context.UserInfo.Include(x => x.User)
                                 on eventCollection.StatKeeperId equals user.Id

                                 join activity in _context.Activity.Include(x => x.ActivityCategory)
                                 //.Where(x => !string.IsNullOrEmpty(basicFilter.sSearch) ? x.Name.ToLower().Contains(basicFilter.sSearch.ToLower().Trim()) : x.Id != 0)

                                 on eventCollection.ActivityId equals activity.Id

                                 join eventTeam in _context.EventTeam
                                 on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                 join events in _context.Event
                                 on eventTeam.EventId equals events.Id

                                 join eventStatus in _context.EventStatus
                                 on eventTeam.EventStatusId equals eventStatus.Id

                                 join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                 on user.Id equals imageTbl.TableId into newImageOuter
                                 from Image in newImageOuter.DefaultIfEmpty()


                                 where Image != null ? Image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() &&
                                 Image.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : venue.Id != 0

                                 join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                 on activity.Id equals imageTbl.TableId into newActivityImageOuter
                                 from ImageActivity in newActivityImageOuter.DefaultIfEmpty()

                                  
                                 

                                 select new GetVenuesEventWithoutTeams
                                 {
                                     Id = events.Id,
                                     VenueName = venue.Name,
                                     venueId = venue.Id,
                                     BranchName = branch.Name,
                                     CreatedDate = eventCollection.CreatedDateTime,
                                     StatskeeperName = user.Name,
                                     StatskeeperImage = Image != null ? Image.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     ActivityIcon = ImageActivity != null ? ImageActivity.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     StatskeeperContact = user.User.Email,
                                     Status = eventStatus.Name,
                                     CurrentActivity = activity.ActivityCategory.Name,
                                 }).Where(x => !string.IsNullOrEmpty(basicFilter.sSearch) ?
                                         (x.VenueName.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())        ||
                                          x.CurrentActivity.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())  ||
                                          x.StatskeeperName.ToLower().Contains(basicFilter.sSearch.ToLower().Trim())  ||
                                          x.BranchName.ToLower().Contains(basicFilter.sSearch.ToLower().Trim()))   :
                                          x.Id != 0)
                                 .OrderByDescending(x => x.Id).Skip((basicFilter.PageNumber - 1) * basicFilter.PageSize)
                                 .Take(basicFilter.PageSize).Distinct().ToListAsync();

            return results;

        }
        public async Task<int> GetVenuesEventWithoutTeamsPlayerCount(long userId, string sSearch)
            {
                List<GetVenuesEventWithoutTeams> VenuesEventListWithoutTeams = new List<GetVenuesEventWithoutTeams>();

                var results = await (from venue in _context.Venue

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



                                     join user in _context.UserInfo.Include(x => x.User)
                                     on eventCollection.StatKeeperId equals user.Id

                                     join activity in _context.Activity.Include(x => x.ActivityCategory)
                                     on eventCollection.ActivityId equals activity.Id

                                     join eventTeam in _context.EventTeam
                                     on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                     join events in _context.Event
                                     on eventTeam.EventId equals events.Id



                                     join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                     on user.Id equals imageTbl.TableId into newImageOuter
                                     from Image in newImageOuter.DefaultIfEmpty()



                                     where Image != null ? Image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() &&
                                     Image.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : venue.Id != 0

                                     join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                     on activity.Id equals imageTbl.TableId into newActivityImageOuter
                                     from ImageActivity in newActivityImageOuter.DefaultIfEmpty()


                                     select new GetVenuesEventWithoutTeams
                                     {
                                         Id = events.Id,
                                         VenueName = venue.Name,
                                         venueId = venue.Id,
                                         BranchName = branch.Name,
                                         CreatedDate = eventCollection.CreatedDateTime,
                                         StatskeeperName = user.Name,
                                         StatskeeperImage = Image != null ? Image.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                         ActivityIcon = ImageActivity != null ? ImageActivity.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                         StatskeeperContact = user.User.Email,
                                         CurrentActivity = activity.ActivityCategory.Name,
                                     }).Where(x => !string.IsNullOrEmpty(sSearch) ?
                                         (x.VenueName.ToLower().Contains(sSearch.ToLower().Trim()) ||
                                          x.CurrentActivity.ToLower().Contains(sSearch.ToLower().Trim()) ||
                                          x.StatskeeperName.ToLower().Contains(sSearch.ToLower().Trim()) ||
                                          x.BranchName.ToLower().Contains(sSearch.ToLower().Trim())) :
                                          x.Id != 0).Distinct().CountAsync();

                return results;
            }
        public async Task<List<GetVenuesEventWithoutTeams>> GetVenuesEventWithoutTeamsStatkeeper(BasicFilter basicFilter, long userId)
            {
                List<GetVenuesEventWithoutTeams> VenuesEventListWithoutTeams = new List<GetVenuesEventWithoutTeams>();

                var results = await (from venue in _context.Venue

                                     join venueStatkeeper in _context.VenueStatkeeper
                                     on venue.Id equals venueStatkeeper.VenueId

                                     where basicFilter.sSearch != "" ? venue.Name.Contains(basicFilter.sSearch) : basicFilter.sSearch == ""
                                     join userInfo in _context.UserInfo
                                      on venueStatkeeper.UserInfoId equals userInfo.Id

                                     where venueStatkeeper.UserInfoId == userId

                                     join eventCollection in _context.EventCollection
                                     on venue.Id equals eventCollection.VenueId

                                     join branch in _context.Branch
                                     on venue.BranchId equals branch.Id

                                     join eventCollectionTeam in _context.EventCollectionTeam
                                     on eventCollection.Id equals eventCollectionTeam.EventCollectionId



                                     join user in _context.UserInfo.Include(x => x.User)
                                     on eventCollection.StatKeeperId equals user.Id

                                     join activity in _context.Activity.Include(x => x.ActivityCategory)
                                     on eventCollection.ActivityId equals activity.Id

                                     join eventTeam in _context.EventTeam
                                     on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId


                                     join events in _context.Event
                                      on eventTeam.EventId equals events.Id


                                     join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                     on user.Id equals imageTbl.TableId into newImageOuter
                                     from Image in newImageOuter.DefaultIfEmpty()



                                     where Image != null ? Image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() &&
                                     Image.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : venue.Id != 0

                                     join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                     on activity.Id equals imageTbl.TableId into newActivityImageOuter
                                     from ImageActivity in newActivityImageOuter.DefaultIfEmpty()



                                     select new GetVenuesEventWithoutTeams
                                     {
                                         Id = events.Id,
                                         VenueName = venue.Name,
                                         venueId = venue.Id,
                                         BranchName = branch.Name,
                                         CreatedDate = eventCollection.CreatedDateTime,
                                         StatskeeperName = user.Name,
                                         StatskeeperImage = Image != null ? Image.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                         ActivityIcon = ImageActivity != null ? ImageActivity.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                         StatskeeperContact = user.User.Email,

                                         CurrentActivity = activity.ActivityCategory.Name,


                                     }).Distinct().OrderByDescending(x => x.Id).Skip((basicFilter.PageNumber - 1) * basicFilter.PageSize)
                                .Take(basicFilter.PageSize).ToListAsync();

                return results;
            }

        public async Task<int> GetVenuesEventWithoutTeamsStatkeeperCount(long userId,string search)
            {
                List<GetVenuesEventWithoutTeams> VenuesEventListWithoutTeams = new List<GetVenuesEventWithoutTeams>();

                var results = await (from venue in _context.Venue

                                     join venueStatkeeper in _context.VenueStatkeeper
                                     on venue.Id equals venueStatkeeper.VenueId

                                     where search != "" ? venue.Name.Contains(search) : search == ""

                                     join userInfo in _context.UserInfo
                                     on venueStatkeeper.UserInfoId equals userInfo.Id

                                     where venueStatkeeper.UserInfoId == userId

                                     join eventCollection in _context.EventCollection
                                     on venue.Id equals eventCollection.VenueId

                                     join branch in _context.Branch
                                     on venue.BranchId equals branch.Id

                                     join eventCollectionTeam in _context.EventCollectionTeam
                                     on eventCollection.Id equals eventCollectionTeam.EventCollectionId



                                     join user in _context.UserInfo.Include(x => x.User)
                                     on eventCollection.StatKeeperId equals user.Id

                                     join activity in _context.Activity.Include(x => x.ActivityCategory)
                                     on eventCollection.ActivityId equals activity.Id

                                     join eventTeam in _context.EventTeam
                                     on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                     join events in _context.Event
                                     on eventTeam.EventId equals events.Id



                                     join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                     on user.Id equals imageTbl.TableId into newImageOuter
                                     from Image in newImageOuter.DefaultIfEmpty()



                                     where Image != null ? Image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() &&
                                     Image.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : venue.Id != 0

                                     join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                     on activity.Id equals imageTbl.TableId into newActivityImageOuter
                                     from ImageActivity in newActivityImageOuter.DefaultIfEmpty()


                                     select new GetVenuesEventWithoutTeams
                                     {
                                         Id = events.Id,
                                         VenueName = venue.Name,
                                         venueId = venue.Id,
                                         BranchName = branch.Name,
                                         CreatedDate = eventCollection.CreatedDateTime,
                                         StatskeeperName = user.Name,
                                         StatskeeperImage = Image != null ? Image.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                         ActivityIcon = ImageActivity != null ? ImageActivity.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                         StatskeeperContact = user.User.Email,

                                         CurrentActivity = activity.ActivityCategory.Name,
                                     }).Distinct().CountAsync();

                return results;
            }

        public async Task<List<GetVenuesEventWithoutTeams>> StatkeeperInVenue(BasicFilter paginationFilter, long userId)
            {


                var Result = await (from venue in _context.Venue
                                    join venueStatkeeper in _context.VenueStatkeeper
                                    on venue.Id equals venueStatkeeper.VenueId

                                    join userInfo in _context.UserInfo
                                    on venueStatkeeper.UserInfoId equals userInfo.Id

                                    join eventCollection in _context.EventCollection
                                    on new { venueStatkeeper.VenueId, venueStatkeeper.UserInfoId.Value } equals new { eventCollection.VenueId, eventCollection.StatKeeperId.Value } into ecGroup
                                    from eventCollectionTbl in ecGroup.DefaultIfEmpty()

                                    join branch in _context.Branch
                                    on venue.BranchId equals branch.Id

                                    where userInfo.Id == userId

                                    /*************/


                                    /*join eventCollectionTeam in _context.EventCollectionTeam
                                    on eventCollectionTbl.Id equals eventCollectionTeam.EventCollectionId*/



                                    /* join user in _context.UserInfo.Include(x => x.User)
                                     on eventCollectionTbl.StatKeeperId equals user.Id*/
                                    /*
                                                                    join activity in _context.Activity.Include(x => x.ActivityCategory)
                                                                    on eventCollectionTbl.ActivityId equals activity.Id

                                                                    join eventTeam in _context.EventTeam
                                                                    on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                                                    join events in _context.Event
                                                                    on eventTeam.EventId equals events.Id

                                                                    join eventStatus in _context.EventStatus
                                                                    on eventTeam.EventStatusId equals eventStatus.Id*/

                                    select new GetVenuesEventWithoutTeams
                                    {
                                        //Id = event.Id
                                        VenueName = venue.Name,
                                        venueId = venue.Id,
                                        BranchName = branch.Name,
                                        // CreatedDate = eventCollectionTbl.CreatedDateTime,
                                        StatskeeperName = userInfo.Name,
                                        // StatskeeperImage = Image != null ? Image.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                        //ActivityIcon = ImageActivity != null ? ImageActivity.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                        StatskeeperContact = userInfo.User.Email,
                                        //Status = eventStatus.Name,
                                        //CurrentActivity = activity.ActivityCategory.Name,
                                    }).ToListAsync();

                return Result;
                /*    var check = await (from venue in _context.Venue
                                       join venueStatKeeper in _context.VenueStatkeeper
                                       on venue.Id equals venueStatKeeper.VenueId

                                       join userInfo in _context.UserInfo
                                       on venueStatKeeper.UserInfoId equals userInfo.Id



                                       where venueStatKeeper.UserInfoId == userId

                                       join eventCollection in _context.EventCollection
                                       on venue.Id equals eventCollection.VenueId

                                       select new StatkeeperInVenue
                                       {
                                           venueId = venue.Id
                                       }).ToListAsync();

                    return check;*/

                /*if (check.Any()){
                    var results = await (from venue in _context.Venue

                                         join venueStatkeeper in _context.VenueStatkeeper
                                         on venue.Id equals venueStatkeeper.VenueId


                                         join userInfo in _context.UserInfo
                                         on venueStatkeeper.UserInfoId equals userInfo.Id

                                         join branch in _context.Branch
                                         on venue.BranchId equals branch.Id

                                         join eventCollection in _context.EventCollection
                                         on venue.Id equals eventCollection.VenueId

                                         join eventCollectionTeam in _context.EventCollectionTeam
                                         on eventCollection.Id equals eventCollectionTeam.EventCollectionId

                                         where venueStatkeeper.UserInfoId == userId

                                         join user in _context.UserInfo.Include(x => x.User)
                                         on eventCollection.StatKeeperId equals user.Id

                                         join activity in _context.Activity.Include(x => x.ActivityCategory)
                                         on eventCollection.ActivityId equals activity.Id

                                         join eventTeam in _context.EventTeam
                                         on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                         join events in _context.Event
                                         on eventTeam.EventId equals events.Id

                                         join eventStatus in _context.EventStatus
                                         on eventTeam.EventStatusId equals eventStatus.Id


                                         join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                         on user.Id equals imageTbl.TableId into newImageOuter
                                         from Image in newImageOuter.DefaultIfEmpty()



                                         where Image != null ? Image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() &&
                                         Image.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : venue.Id != 0

                                         join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                         on activity.Id equals imageTbl.TableId into newActivityImageOuter
                                         from ImageActivity in newActivityImageOuter.DefaultIfEmpty()


                                         select new GetVenuesEventWithoutTeams
                                         {
                                             Id = events.Id,
                                             VenueName = venue.Name,
                                             venueId = venue.Id,
                                             BranchName = branch.Name,
                                             CreatedDate = eventCollection.CreatedDateTime,
                                             StatskeeperName = userInfo.Name,
                                             StatskeeperImage = Image != null ? Image.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                             ActivityIcon = ImageActivity != null ? ImageActivity.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                             StatskeeperContact = userInfo.User.Email,
                                             Status = eventStatus.Name,
                                             CurrentActivity = activity.ActivityCategory.Name,
                                         }).ToListAsync();
                    return results;
                }*/
                /*else{
                    var results = await (from venue in _context.Venue

                                         join venueStatkeeper in _context.VenueStatkeeper
                                         on venue.Id equals venueStatkeeper.VenueId


                                         join userInfo in _context.UserInfo
                                         on venueStatkeeper.UserInfoId equals userInfo.Id

                                         join branch in _context.Branch
                                         on venue.BranchId equals branch.Id

                                         join eventCollection in _context.EventCollection
                                         on venue.Id equals eventCollection.VenueId

                                         *//*join eventCollectionTeam in _context.EventCollectionTeam
                                         on eventCollection.Id equals eventCollectionTeam.EventCollectionId*//*

                                         where venueStatkeeper.UserInfoId == userId

                                         *//*join user in _context.UserInfo.Include(x => x.User)
                                         on eventCollection.StatKeeperId equals user.Id

                                         join activity in _context.Activity.Include(x => x.ActivityCategory)
                                         on eventCollection.ActivityId equals activity.Id

                                         join eventTeam in _context.EventTeam
                                         on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                         join events in _context.Event
                                         on eventTeam.EventId equals events.Id

                                         join eventStatus in _context.EventStatus
                                         on eventTeam.EventStatusId equals eventStatus.Id


                                         join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                         on user.Id equals imageTbl.TableId into newImageOuter
                                         from Image in newImageOuter.DefaultIfEmpty()



                                         where Image != null ? Image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower() &&
                                         Image.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() : venue.Id != 0

                                         join imageTbl in _context.Image.AsNoTracking().Include(x => x.ImageType).Include(x => x.ConnectionEntity)
                                         on activity.Id equals imageTbl.TableId into newActivityImageOuter
                                         from ImageActivity in newActivityImageOuter.DefaultIfEmpty()
        *//*

                                         select new GetVenuesEventWithoutTeams
                                         {
                                             //Id = events.Id,
                                             VenueName = venue.Name,
                                             venueId = venue.Id,
                                             BranchName = branch.Name,
                                             CreatedDate = eventCollection.CreatedDateTime,
                                             StatskeeperName = userInfo.Name,
                                             //StatskeeperImage = Image != null ? Image.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                             //ActivityIcon = ImageActivity != null ? ImageActivity.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                             StatskeeperContact = userInfo.User.Email,
                                             //Status = eventStatus.Name,
                                             //CurrentActivity = activity.ActivityCategory.Name,
                                         }).ToListAsync();
                    return results;
                }*/

            }
        public async Task<int> GetVenuesEventCount(long userId)
            {
                List<GetVenuesEventWithoutTeams> VenuesEventListWithoutTeams = new List<GetVenuesEventWithoutTeams>();

                var results = await (from eventCollection in _context.EventCollection.Where(x => x.IsDeleted != true && x.UserInfo.Id == userId)
                                     join eventCollectionTeam in _context.EventCollectionTeam
                                     on eventCollection.Id equals eventCollectionTeam.EventCollectionId

                                     join venue in _context.Venue
                                     on eventCollection.VenueId equals venue.Id

                                     join branch in _context.Branch
                                     on venue.BranchId equals branch.Id

                                     join user in _context.UserInfo.Include(x => x.User)
                                     on eventCollection.StatKeeperId equals user.Id

                                     join activity in _context.Activity
                                     on eventCollection.ActivityId equals activity.Id

                                     join eventTeam in _context.EventTeam
                                     on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                     join eventStatus in _context.EventStatus
                                     on eventTeam.EventStatusId equals eventStatus.Id

                                     group eventCollection by eventCollection.Id into g

                                     select new BranchesVenuesResponse
                                     {
                                         Id = g.Key
                                     }).Distinct().CountAsync();

                return results;

            }

        public async Task<List<GetAllTeamsByVenueId>> GetAllTeamsByVenueId(List<long> venueid)
            {

                var results = await (from venue in _context.Venue
                                     join eventCollection in _context.EventCollection
                                     on venue.Id equals eventCollection.VenueId

                                     join eventCollectionTeam in _context.EventCollectionTeam
                                     on eventCollection.Id equals eventCollectionTeam.EventCollectionId

                                     join team in _context.Team
                                     on eventCollectionTeam.TeamId equals team.Id

                                     join teamMember in _context.TeamMember
                                     on team.Id equals teamMember.TeamId

                                     group new { teamMember, eventCollection } by teamMember.Id into g

                                     select new GetAllTeamsByVenueId
                                     {
                                         Id = g.Key,
                                         EventCollectionId = g.First().eventCollection.Id,
                                         Name = g.First().teamMember.Team.Name,
                                         Members = g.Count()
                                     }).ToListAsync();


                return results;


            }

        public async Task<BranchesVenuesListById> GetBranchesVenuesListById(long EventId)
            {

                BranchesVenuesListById BranchesVenuesListById = new BranchesVenuesListById();

                BranchesVenuesListById = await (from eventCollection in _context.EventCollection.Where(x => x.Id == EventId)
                                                join venue in _context.Venue
                                                on eventCollection.VenueId equals venue.Id

                                                join branch in _context.Branch
                                                on venue.BranchId equals branch.Id

                                                join userInfo in _context.UserInfo
                                                on eventCollection.StatKeeperId equals userInfo.Id

                                                join activity in _context.Activity
                                                on eventCollection.ActivityId equals activity.Id





                                                select new BranchesVenuesListById
                                                {
                                                    Id = eventCollection.Id,
                                                    BranchId = branch.Id,
                                                    VenueName = venue.Name,
                                                    StatsKeeperId = userInfo.Id,
                                                    ActivityId = activity.Id
                                                }).FirstOrDefaultAsync();
                return BranchesVenuesListById;
            }

        public async Task<BranchesVenuesListById> UpdateEventList(UpdateEventVenueListReq VenueEventListUpdateReq)
            {
                /*BranchesVenuesListById results = await (from venue in _context.Venue.Where(x=>x.Id== VenueEventListUpdateReq.Id)
                                                join branch in _context.Branch
                                                on venue.BranchId equals branch.Id

                                                join venuesStatskeeper in _context.VenueStatkeeper
                                                on venue.Id equals venuesStatskeeper.VenueId

                                                join usersInfo in _context.UserInfo
                                                on venuesStatskeeper.UserInfoId equals usersInfo.Id

                                                join venueActivity in _context.VenueActivity
                                                on venue.Id equals venueActivity.VenueId

                                                join activity in _context.Activity
                                                on venueActivity.ActivityId equals activity.Id

                                                select new BranchesVenuesListById
                                                {
                                                    Id = venue.Id,
                                                    BranchId = branch.Id,
                                                    VenueName = venue.Name,
                                                    StatsKeeperId = usersInfo.Id,
                                                    ActivityId = activity.Id
                                                }).FirstOrDefaultAsync();
                return results;*/

                var result = await (from eventCollection in _context.EventCollection.Where(x => x.Id == VenueEventListUpdateReq.Id)
                                    join venue in _context.Venue
                                    on eventCollection.VenueId equals venue.Id

                                    join branch in _context.Branch
                                    on venue.BranchId equals branch.Id

                                    join userInfo in _context.UserInfo
                                    on eventCollection.StatKeeperId equals userInfo.Id

                                    join activity in _context.Activity
                                    on eventCollection.ActivityId equals activity.Id


                                    select new BranchesVenuesListById
                                    {
                                        Id = VenueEventListUpdateReq.Id,
                                        StatsKeeperId = VenueEventListUpdateReq.StatsKeeperId,
                                        ActivityId = VenueEventListUpdateReq.activityId

                                    }).FirstOrDefaultAsync();
                return result;
                /*  if (venue == null)
                  {
                      // Handle the case where the venue ID is not found
                      return null;
                  }

                  // Update the fields against that venueid

                  venue.Id = VenueEventListUpdateReq.StatsKeeperId; // Example of updating the name field

                  // Save the changes to the database
                  //await _context.SaveChangesAsync();

                  // Return the updated venue object
                  return new BranchesVenuesListById
                  {
                      Id = venue.Id,

                      VenueName = venue.Name
                      // Add other fields as needed
                  };*/
            }

        public async Task<long> EventStatusId(Event Event)
            {
                var result = await (from eventTeam in _context.EventTeam
                                    where eventTeam.Event.Id == Event.Id

                                    join eventStatus in _context.EventStatus
                                    on eventTeam.EventStatusId equals eventStatus.Id

                                    select eventStatus.Id).FirstOrDefaultAsync();
                return result;
            }

        public async Task<long> EventTeamId(Domain.Entities.Event Event)
            {
                var result = await (from eventTeam in _context.EventTeam
                                    where eventTeam.Event.Id == Event.Id

                                    join eventCollectionTeam in _context.EventCollectionTeam
                                    on eventTeam.EventCollectionTeamId equals eventCollectionTeam.Id

                                    join eventCollection in _context.EventCollection
                                    on eventCollectionTeam.EventCollectionId equals eventCollection.Id

                                    select eventCollection.Id).FirstOrDefaultAsync();
                return result;
            }

        public async Task<Ensuranx.Domain.Entities.EventCollection> UpdateEventCollection(Ensuranx.Domain.Entities.EventCollection EventCollection)
            {
                CancellationToken cancellationToken = CancellationToken.None;
                await _iunitOfWork.Repository<Ensuranx.Domain.Entities.EventCollection>().UpdateAsync(EventCollection);
                await _iunitOfWork.Commit(cancellationToken);
                return EventCollection;
            }
        public async Task<List<GetAllPlayersByEC>> GetAllPlayers(List<long> eventId)
            {

                var query = await (from events in _context.Event
                                   join eventTeam in _context.EventTeam
                                   on events.Id equals eventTeam.EventId

                                   join eventCollectionTeam in _context.EventCollectionTeam
                                   on eventTeam.EventCollectionTeamId equals eventCollectionTeam.Id

                                   join team in _context.Team
                                   on eventCollectionTeam.TeamId equals team.Id

                                   join teamMember in _context.TeamMember
                                   on team.Id equals teamMember.TeamId

                                   join eventCollection in _context.EventCollection on eventCollectionTeam.EventCollectionId equals eventCollection.Id
                                   join venue in _context.Venue on eventCollection.VenueId equals venue.Id
                                   where teamMember.TeamId == team.Id &&
                                         eventCollection.Id == eventCollectionTeam.EventCollectionId &&
                                         venue.Id == eventTeam.EventCollectionTeam.EventCollection.VenueId

                                   where eventId.Contains(events.Id)

                                   group teamMember by new { events.Id } into g

                                   select new GetAllPlayersByEC
                                   {
                                       Id = g.Key.Id,
                                       TeamCount = g.Select(x => x.TeamId).Distinct().Count(),
                                       PlayerCount = g.LongCount()
                                   }).ToListAsync();

                return query;
            }

        public async Task<List<GetAllPlayersByEC>> GetAllPlayersByPlayer(List<long> eventId)
            {

                var query = await (from events in _context.Event
                                   join eventTeam in _context.EventTeam
                                   on events.Id equals eventTeam.EventId

                                   join eventCollectionTeam in _context.EventCollectionTeam
                                   on eventTeam.EventCollectionTeamId equals eventCollectionTeam.Id

                                   join team in _context.Team
                                   on eventCollectionTeam.TeamId equals team.Id

                                   join teamMember in _context.TeamMember
                                   on team.Id equals teamMember.TeamId


                                   join eventCollection in _context.EventCollection on eventCollectionTeam.EventCollectionId equals eventCollection.Id
                                   join venue in _context.Venue on eventCollection.VenueId equals venue.Id
                                   where teamMember.TeamId == team.Id &&
                                          eventCollection.Id == eventCollectionTeam.EventCollectionId &&
                                          venue.Id == eventTeam.EventCollectionTeam.EventCollection.VenueId

                                   where eventId.Contains(events.Id)
                                   group teamMember by new { events.Id } into g
                                   select new GetAllPlayersByEC
                                   {
                                       Id = g.Key.Id,
                                       TeamCount = g.Select(x => x.TeamId).Distinct().Count(),
                                       PlayerCount = g.LongCount()
                                   }).ToListAsync();

                return query;
            }

        public async Task<List<VenueByBrandIdResp>> GetVenuesDetail(long branchId)
        {
            var results = await (from venue in _context.Venue.Where(x => x.BranchId == branchId)
  
                                 select new VenueByBrandIdResp
                                 {
                                     Id = venue.Id,
                                     Name = venue.Name,
                                     CreatedDate = venue.CreatedDateTime,
                                 }
                                 ).ToListAsync();
            return results;

        }

        public async Task<List<StatkeeperImagesDetails>> GetStatkeeperDetails(List<long> venueId)
        {
            var results = await (from venuesStatskeeper in _context.VenueStatkeeper
                                 join userInfo in _context.UserInfo on venuesStatskeeper.UserInfoId equals userInfo.Id

                                 where venueId.Contains(venuesStatskeeper.VenueId)
                                 join imageTbl in _context.Image.AsNoTracking()
                                          .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                                      x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                                      on venuesStatskeeper.UserInfoId equals imageTbl.TableId into newFullImage
                                 from imageTblFull in newFullImage.DefaultIfEmpty()

                                 select new StatkeeperImagesDetails
                                 {
                                     BranchId=venuesStatskeeper.VenueId,
                                     Id=(long)venuesStatskeeper.UserInfoId,
                                     Name=venuesStatskeeper.UserInfo.Name,
                                     ProfileImage= imageTblFull != null ? imageTblFull.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                 }
                                 ).ToListAsync();

            return results;
        }

        public async Task<List<currentActivityResp>> GetCurrentActivity(List<long> venueId)
        {
           // string v = Domain.Enums.Enum.EventStatus.Active.ToString();
           // string x = Domain.Enums.Enum.EventStatus.Active;
            var results = await (from ec in _context.EventCollection
                                 where venueId.Contains(ec.VenueId)
                                 join eventCollectionTeam in _context.EventCollectionTeam
                                 on ec.Id equals eventCollectionTeam.EventCollectionId

                                 join activty in _context.Activity
                                 on ec.ActivityId equals activty.Id

                                 join eventTeam in _context.EventTeam
                                 on eventCollectionTeam.Id equals eventTeam.EventCollectionTeamId

                                 join events in _context.Event
                                 on eventTeam.EventId equals events.Id

                               

                                 join imageTbl in _context.Image.AsNoTracking()
                                          .Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower() &&
                                                      x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                                      on ec.ActivityId equals imageTbl.TableId into newFullImage
                                 from imageTblFull in newFullImage.DefaultIfEmpty()

                                where eventTeam.EventStatus.Name == Domain.Enums.Enum.EventStatus.Active.ToString()

                                select new currentActivityResp
                                             {
                                                 venueId= ec.VenueId,
                                                 eventId = events.Id,
                                                 currentActId= activty.Id,
                                                 currentActName= activty.Name,
                                                 CurrentActivityImageUrl= imageTblFull != null ? imageTblFull.Path : Common.Constants.Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                             }).Distinct().ToListAsync();

            return results;
        }

    }
} 


