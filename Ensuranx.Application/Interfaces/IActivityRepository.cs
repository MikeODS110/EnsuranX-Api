using Ensuranx.Application.Response.Activity;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IActivityRepository
    {
        public Task<List<ActivityResponse>> GetAllActivityAsyncByBusiness(BasicFilter paginationFilter, long userId, long venueId);
        public Task<List<ActivityResponse>> GetAllActivityByStatkeeper(BasicFilter paginationFilter, long userId,long venueId);
        public Task<int> GetAllActivityCountByStatkeeper(long userId);
        public Task<Activity> GetActivityByEventTypeAndActivityCategoryId(long eventTypeId,long activityCategoryId);
        public Task<List<Activity>> GetActivityListByEventTypeAndActivityCategoryId(List<long> activityCategoryIdList);
        public Task<int> GetMaxTeamMemberCountByTeamId(long teamId);
        public Task<int> GetAllActivityCount();
    }
}
