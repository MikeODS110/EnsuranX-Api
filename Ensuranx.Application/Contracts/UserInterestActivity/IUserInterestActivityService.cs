using ErrorOr;
using Ensuranx.Application.Requests.Profile;
using Ensuranx.Application.Response.User;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Contracts.UserInterestActivity
{
    public interface IUserInterestActivityService
    {
        public Task<ErrorOr<PlayerInterestActivity>> AddNewPlayerInterestActivity(PlayerInterestActivity playerInterestActivity);
        public Task<ErrorOr<List<PlayerInterestActivity>>> AddNewPlayerInterestActivityList(UserInterestsActivities userInterestsActivities, long userId,string email);
        public Task<ErrorOr<PlayerInterestsResponse?>> GetUserInterestsById(long userId);
    }
}
