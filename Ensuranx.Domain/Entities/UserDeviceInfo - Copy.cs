using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities;

public class UserDeviceInfo : AuditableEntity<long>
{
    public virtual long UserInfoId { get; set; }
    [ForeignKey("UserInfoId")]
    public virtual UserInfo UserInfo { get; set; }
    public string DeviceId { get; set; }
    public string DeviceFCMToken { get; set; }
    public Domain.Enums.Enum.Platform Platform { get; set; }
    public bool IsInUse { get; set; } = false;
}
