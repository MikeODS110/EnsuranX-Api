using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers;

public class PermissionMapper
{
    public List<UserPermission> MapUserWithPermissionPermissionTypeId(long userInfoId,string email,List<long> permissionPermissionTypeIdList)
    {
        List<UserPermission> userPermissionList = new List<UserPermission>();

        foreach (long permissionPermissionTypeId in permissionPermissionTypeIdList)
        {
            UserPermission userPermission = new UserPermission();

            userPermission.UserInfoId = userInfoId;
            userPermission.PermissionPermissionTypeId = permissionPermissionTypeId;
            userPermission.CreatedBy = email;
            userPermission.LastModifiedBy = email;
            userPermission.CreatedDateTime = DateTime.UtcNow;
            userPermission.LastModifiedDateTime = DateTime.UtcNow;

            userPermissionList.Add(userPermission);
        }
        return userPermissionList;
    }
}
