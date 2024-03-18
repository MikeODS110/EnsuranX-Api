using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces;

public interface IFollowRepository
{
    public Task<List<Follow>> GetAllFollowerOrFolloweeList(long userId, List<long> totalTeamMemberList);
    public Task<long> GetFollowersCountForThisUser(long userId);
    public Task<long> GetFollowingCountForThisUser(long userId);
    public Task<Follow> GetFollowerByFollowerAndFolloweeId(long userId,long followerId);
    public IQueryable<Follow> GetAllFollowRequestForThisUserNotAcceptedYet(long userId);
    public IQueryable<Follow> GetAllFollowersForThisUserAccepted(long userId);
}
