using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class UserPermission : AuditableEntity<long>
    {
        public virtual long? UserInfoId { get; set; }
        [ForeignKey("UserInfoId")]
        public virtual UserInfo UserInfo { get; set; }
        public virtual long PermissionPermissionTypeId { get; set; }
        [ForeignKey("PermissionPermissionTypeId")]
        public virtual PermissionPermissionType PermissionPermissionType { get; set; }
    }
}
