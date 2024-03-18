using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities;
public class PermissionPermissionType : AuditableEntity<long>
{
    public virtual long PermissionId { get; set; }
    [ForeignKey("PermissionId")]
    public virtual Permission Permission { get; set; }
    public virtual long PermissionTypeId { get; set; }
    [ForeignKey("PermissionTypeId")]
    public virtual PermissionType PermissionType { get; set; }
}
