using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces;

public interface IUserPermissionRepository
{
    public Task<List<UserPermission>> GetUserPermissionByUserIdAsync(long userId);
    public Task<List<PermissionPermissionType>> GetPermissionPermissionTypeAsync();
    public Task<List<UserPermission>> DeleteUserPermissionAsync(List<UserPermission> userPermissionList);
    public Task<List<UserPermission>> DeleteUserPermissionListByUserIdAsync(long userId);
    public Task<List<UserPermission>> CreateUserPermissionAsync(List<UserPermission> userPermissionList);
}
