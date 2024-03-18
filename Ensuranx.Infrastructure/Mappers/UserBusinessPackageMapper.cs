using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class UserBusinessPackageMapper
    {
        public UserBusinessPackage MapUserBusinessPackageToCreate(string email,long userId,long businessPackageId)
        {
            UserBusinessPackage userBusinessPackage = new UserBusinessPackage();

            userBusinessPackage.CreatedBy = email;
            userBusinessPackage.LastModifiedBy = email;
            userBusinessPackage.CreatedDateTime = DateTime.UtcNow;
            userBusinessPackage.LastModifiedDateTime = DateTime.UtcNow;
            userBusinessPackage.UserInfoId = userId;
            userBusinessPackage.BusinessPackageId = businessPackageId;

            return userBusinessPackage;
        }

        public UserBusinessPackage MapUserBusinessPackageToUpdate(string email, long userId, UserBusinessPackage userBusinessPackage,long businessPackageId)
        {
            //userBusinessPackage.CreatedBy = email;
            userBusinessPackage.LastModifiedBy = email;
            //userBusinessPackage.CreatedDateTime = DateTime.UtcNow;
            userBusinessPackage.LastModifiedDateTime = DateTime.UtcNow;
            userBusinessPackage.UserInfoId = userId;
            userBusinessPackage.BusinessPackageId = businessPackageId;

            return userBusinessPackage;
        }
    }
}
