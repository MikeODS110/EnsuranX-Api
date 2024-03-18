using Ensuranx.Application.Requests.Profile;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IPermissionPermissionTypeRepository
    {
        public Task<List<PermissionPermissionType>> GetPermissionPermissionTypeListForUpdate(UpdatePermission updatePermission);
        public Task<List<PermissionPermissionType>> GetPermissionPermissionTypeListByPermissionIdListAndPermissionTypeId(List<long> permissionIdList,long permissionTypeId);
        public Task<PermissionType> GetPermissionTypeByPermissionTypeName(string name);
    }
}
