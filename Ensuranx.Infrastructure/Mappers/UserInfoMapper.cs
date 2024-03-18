using Ensuranx.Application.Response.User;
using Ensuranx.Domain.Entities;
using Ensuranx.Domain.IdentityExtensions;
using System.Collections.Generic;

namespace Ensuranx.Infrastructure.Mappers
{
    public class UserInfoMapper
    {
        public UserInfo SetDataInUserInfo(AppUser appUser, int OtpCode, string Name, int ZipCode)
        {
            UserInfo userInfo = new UserInfo();

            userInfo.UserId = appUser.Id;
            userInfo.OTPCode = OtpCode;
            userInfo.IsSend = false;
            userInfo.DateOfRegisteration = DateTime.UtcNow;
            userInfo.CreatedBy = appUser.Email;
            userInfo.LastModifiedBy = appUser.Email;
            userInfo.CreatedDateTime = DateTime.UtcNow;
            userInfo.LastModifiedDateTime = DateTime.UtcNow;
            userInfo.Name = Name;
            userInfo.ZipCode = ZipCode;

            return userInfo;
        }
       


        public PermissionResponse MappingPermissionsDropdownList(List<UserPermission> userPermissionList, List<PermissionPermissionType> permissionPermissionTypeList)
        {
            PermissionResponse permissionResponse = new PermissionResponse();
            permissionResponse.PermissionDetailList = new List<PermissionDetail>();

            foreach (Domain.Enums.Enum.Permission permission in System.Enum.GetValues(typeof(Domain.Enums.Enum.Permission)))
            {
                PermissionDetail permissionDetail = new PermissionDetail();

                List<PermissionPermissionType> permissionPermissionTypeRes = permissionPermissionTypeList
                                                                             .Where(x => x.Permission.Name.ToLower() == permission.ToString().ToLower()).ToList();
                if (permissionPermissionTypeRes.Any())
                {
                    permissionDetail.PermissionId = permissionPermissionTypeRes.FirstOrDefault().Permission.Id;
                    permissionDetail.PermissionName = permissionPermissionTypeRes.FirstOrDefault().Permission.Name;

                    permissionDetail.PermissionList = new List<Dropdown>();

                    foreach (var permissionType in permissionPermissionTypeRes)
                    {
                        Dropdown dropdown = new Dropdown();

                        dropdown.Text = permissionType.PermissionType.Name;
                        dropdown.Value = permissionType.PermissionType.Id;

                        var getPermission = userPermissionList.Where(x => x.PermissionPermissionTypeId == permissionType.Id).FirstOrDefault();
                        dropdown.Selected = getPermission is not null ? true : false;

                        permissionDetail.PermissionList.Add(dropdown);
                    }

                    permissionResponse.PermissionDetailList.Add(permissionDetail);
                }
            }

            return permissionResponse;
        }


        public List<UserPermission> MapUserInfoWithUpdatedPermissions(List<PermissionPermissionType> permissionPermissionTypeList, long userInfoId, string email)
        {
            List<UserPermission> userPermissionList = new List<UserPermission>();

            for (int i = 0; i < permissionPermissionTypeList.Count; i++)
            {
                UserPermission userPermission = new UserPermission();

                userPermission.PermissionPermissionTypeId = permissionPermissionTypeList[i].Id;
                userPermission.CreatedBy = email;
                userPermission.LastModifiedBy = email;
                userPermission.CreatedDateTime = DateTime.UtcNow;
                userPermission.LastModifiedDateTime = DateTime.UtcNow;
                userPermission.UserInfoId = userInfoId;

                userPermissionList.Add(userPermission);
            }

            return userPermissionList;
        }

       

       
    }
}
