using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories;

public class FollowRepository : IFollowRepository
{
    private readonly ApplicationDbContext _context;

    public FollowRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Follow>> GetAllFollowerOrFolloweeList(long userId, List<long> playerProfileImageList)
    {
        return await _context.Follow.AsNoTracking().Include(x => x.UserInfo).Include(x => x.UserInfo2)
                   .Where(x => (x.FolloweeId == userId || x.FollowerId == userId)
                           && x.IsDeleted != true && x.IsActive != false
                           && (!playerProfileImageList.Contains(x.FolloweeId.Value) || !playerProfileImageList.Contains(x.FollowerId.Value)) 
                           && x.Status != 0).ToListAsync();
    }


    public async Task<long> GetFollowersCountForThisUser(long userId)
    {
        return await _context.Follow.AsNoTracking().Where(x => x.FolloweeId == userId && x.Status != 0).CountAsync();
    }

    public async Task<long> GetFollowingCountForThisUser(long userId)
    {
        return await _context.Follow.AsNoTracking().Where(x => x.FollowerId == userId && x.Status != 0).CountAsync();
    }

    public IQueryable<Follow> GetAllFollowRequestForThisUserNotAcceptedYet(long userId)
    {
        return _context.Follow.AsNoTracking().Where(x => x.FolloweeId == userId && x.Status != 1).AsQueryable();
    }

    public IQueryable<Follow> GetAllFollowersForThisUserAccepted(long userId)
    {
        return _context.Follow.AsNoTracking().Where(x => x.FollowerId == userId && x.Status != 0).AsQueryable();
    }

    public async Task<Follow> GetFollowerByFollowerAndFolloweeId(long userId, long followerId)
    {
        return await _context.Follow.AsNoTracking().Where(x => x.FollowerId == followerId && x.FolloweeId == userId).FirstOrDefaultAsync();
    }
}
