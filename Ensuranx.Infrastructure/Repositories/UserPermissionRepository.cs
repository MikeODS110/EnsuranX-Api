using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories;

public class UserPermissionRepository : IUserPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public UserPermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserPermission>> GetUserPermissionByUserIdAsync(long userId)
    {
        return await _context.UserPermission.Where(p => p.UserInfoId == userId && p.IsDeleted != true && p.IsActive != false)
                     .Include(x => x.PermissionPermissionType)
                     .ThenInclude(x => x.Permission)
                     .Include(x => x.PermissionPermissionType.PermissionType)
                     .ToListAsync();
    }

    public async Task<List<UserPermission>> DeleteUserPermissionAsync(List<UserPermission> userPermissionList)
    {
        _context.UserPermission.RemoveRange(userPermissionList);
        await _context.SaveChangesAsync();
        return userPermissionList;
    }

    public async Task<List<UserPermission>> DeleteUserPermissionListByUserIdAsync(long userId)
    {
        List<UserPermission> userPermissionList = await _context.UserPermission.Where(x => x.UserInfoId == userId).ToListAsync();
        userPermissionList = await DeleteUserPermissionAsync(userPermissionList);
        return userPermissionList;
    }

    public async Task<List<UserPermission>> CreateUserPermissionAsync(List<UserPermission> userPermissionList)
    {
        await _context.UserPermission.AddRangeAsync(userPermissionList);
        await _context.SaveChangesAsync();

        return userPermissionList;
    }

    public async Task<List<PermissionPermissionType>> GetPermissionPermissionTypeAsync()
    {
        return await _context.PermissionPermissionType.AsNoTracking().Where(p => p.IsDeleted != true && p.IsActive != false)
                     .Include(x => x.Permission)
                     .Include(x => x.PermissionType)
                     .ToListAsync();
    }
}
