using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Activity;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;

namespace Ensuranx.Infrastructure.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public ActivityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ActivityResponse>> GetAllActivityAsyncByBusiness(BasicFilter paginationFilter, long userId, long venueId)
        {
            var activityQuery = await (from activity in _context.Activity.Include(x => x.ActivityCategory).AsNoTracking()
                                       join venueActivity in _context.VenueActivity
                                       on activity.Id equals venueActivity.ActivityId

                                       join imageTbl in _context.Image.AsNoTracking()
                                       on activity.Id equals imageTbl.TableId into newFullImage
                                       from ImageFullTbl in newFullImage.DefaultIfEmpty()

                                       where venueId != 0 ? venueActivity.VenueId == venueId : activity.Id != 0
                                       select new
                                       {
                                           Id = activity.ActivityCategory.Id,
                                           Name = activity.ActivityCategory.Name,
                                           Image = ImageFullTbl != null ? ImageFullTbl.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                           activityCategory = activity.ActivityCategory
                                       }).GroupBy(x => x.activityCategory.Id).Select(x => new ActivityResponse
                                       {
                                           Id = x.Key,
                                           Name = x.First().Name,
                                           Image = x.First().Image,
                                       }).ToListAsync();

            return activityQuery;
        }
        public async Task<int> GetAllActivityCount()
        {
            return await (from activity in _context.Activity
                          join venueActivity in _context.VenueActivity
                          on activity.Id equals venueActivity.ActivityId
                          select activity.Id
                          ).Distinct().CountAsync();
        }

        public async Task<List<ActivityResponse>> GetAllActivityByStatkeeper(BasicFilter paginationFilter, long userId, long venueId)
        {
            var activityQuery = await (from activity in _context.Activity.Include(x => x.ActivityCategory).AsNoTracking()
                                       join imageTbl in _context.Image.AsNoTracking()
                                       on activity.Id equals imageTbl.TableId into newFullImage
                                       from ImageFullTbl in newFullImage.DefaultIfEmpty()

                                       join venueActivityTbl in _context.VenueActivity.AsNoTracking()
                                       on activity.Id equals venueActivityTbl.ActivityId into newVenueActivityFull
                                       from venueActivityFullTbl in newVenueActivityFull.DefaultIfEmpty()

                                       where venueId != 0 ? venueActivityFullTbl.VenueId == venueId : activity.Id != 0

                                       select new
                                       {
                                           Id = activity.ActivityCategory.Id,
                                           Name = activity.ActivityCategory.Name,
                                           Image = ImageFullTbl != null ? ImageFullTbl.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                           activityCategory = activity.ActivityCategory
                                       }).GroupBy(x => x.activityCategory.Id).Select(x => new ActivityResponse
                                       {
                                           Id = x.Key,
                                           Name = x.First().Name,
                                           Image = x.First().Image,
                                       }).ToListAsync();

            return activityQuery;
        }
        public async Task<int> GetAllActivityCountByStatkeeper(long venueId)
        {
            var activityQuery = await (from activity in _context.Activity
                                       join venueActivity in _context.VenueActivity
                                       on activity.Id equals venueActivity.ActivityId

                                       where venueActivity.VenueId == venueId
                                       select activity
                                ).CountAsync();

            return activityQuery;
        }

        public async Task<Activity> GetActivityByEventTypeAndActivityCategoryId(long eventTypeId, long activityCategoryId)
        {
            return await (from activityCategoryTbl in _context.ActivityCategory.AsNoTracking()
                          join activityTbl in _context.Activity.AsNoTracking()
                          on activityCategoryTbl.Id equals activityTbl.ActivityCategoryId

                          join evenTypeTbl in _context.EventType.AsNoTracking()
                          on eventTypeId equals evenTypeTbl.Id

                          where activityCategoryTbl.Id == activityCategoryId

                          where evenTypeTbl.Name.ToLower() == Domain.Enums.Enum.EventType.Pickup.ToString().ToLower()
                          ? activityTbl.Name.ToLower().Contains(Domain.Enums.Enum.EventType.Pickup.ToString().ToLower()) :
                          activityTbl.Name.ToLower().Contains(Domain.Enums.Enum.EventType.Regulation.ToString().ToLower())

                          select activityTbl).FirstOrDefaultAsync();
        }

        public async Task<List<Activity>> GetActivityListByEventTypeAndActivityCategoryId(List<long> activityCategoryIdList)
        {
            return await (from activityCategoryTbl in _context.ActivityCategory.AsNoTracking()
                          join activityTbl in _context.Activity.AsNoTracking()
                          on activityCategoryTbl.Id equals activityTbl.ActivityCategoryId

                          where activityCategoryIdList.Contains(activityCategoryTbl.Id)

                          select activityTbl).ToListAsync();
        }


        public async Task<int> GetMaxTeamMemberCountByTeamId(long teamId)
        {
            var teamObj = await _context.Team.Include(x => x.TeamType).AsNoTracking().Where(x => x.Id == teamId).FirstOrDefaultAsync();

            if (teamObj is not null && teamObj.TeamType.Name.ToLower() == Domain.Enums.Enum.TeamType.Official.ToString().ToLower())
            {
                return await _context.TeamActivity.Where(x => x.TeamId == teamId).Select(x => x.Activity.MinPlayerPerTeam).FirstOrDefaultAsync();
            }
            else if (teamObj is not null && teamObj.TeamType.Name.ToLower() == Domain.Enums.Enum.TeamType.Temporary.ToString().ToLower())
            {
                return await _context.EventCollectionTeam.AsNoTracking().Where(x => x.TeamId == teamId).Select(x => x.EventCollection.Activity.MinPlayerPerTeam).FirstOrDefaultAsync();
            }
            else
            {
                return 0;
            }
        }
    }
}
