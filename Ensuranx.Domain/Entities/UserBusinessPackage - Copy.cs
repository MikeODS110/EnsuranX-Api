using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities;

public class UserBusinessPackage : AuditableEntity<long>
{
    public virtual long UserInfoId { get; set; }
    [ForeignKey("UserInfoId")]
    public virtual UserInfo UserInfo { get; set; }

    public virtual long? BusinessPackageId { get; set; }
    [ForeignKey("BusinessPackageId")]
    public virtual BusinessPackages BusinessPackage { get; set; }
}
