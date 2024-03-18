using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IPlayerInterestActivityRepository
    {
        public Task<List<PlayerInterestActivity>> GetPlayerInterestsAcititiesById(long userId);
        public Task<List<MemberDetail>> GetPlayerSuggestionBasedOnInterestMatch(List<long> activityCategoryIdList,long userId, BasicFilter basicFilter, IQueryable<Follow> followerList,long loginUserId);
        public Task<int> GetPlayerSuggestionBasedOnInterestMatchCount(List<long> activityCategoryIdList,long userId, IQueryable<Follow> followerList,long loginUserId);
        public Task<bool> DeleteUserInterest(long userId);
    }
}
