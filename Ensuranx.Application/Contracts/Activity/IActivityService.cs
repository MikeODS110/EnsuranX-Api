using ErrorOr;
using Ensuranx.Application.Response.Activity;
using Ensuranx.Common.PaginationResponse;

namespace Ensuranx.Application.Contracts.Activity
{
    public interface IActivityService
    {
        public Task<ErrorOr<List<ActivityResponse>>> GetAllActivitiesAsync(BasicFilter paginationFilter, long userId,long roleId,long venueId);
    }
}
