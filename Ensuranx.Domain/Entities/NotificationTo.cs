using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class NotificationTo : AuditableEntity<long>
    {
        public virtual long NotificationsId { get; set; }
        [ForeignKey("NotificationsId")]
        public virtual Notifications Notifications { get; set; }
        public virtual long? UserInfoId { get; set; }
        [ForeignKey("UserInfoId")]
        public virtual UserInfo? UserInfo { get; set; }
        public string Email { get; set; } = string.Empty;
        public long? TableId { get; set; }
        public bool IsView { get; set; } = false;
        public bool IsOpen { get; set; } = false;
        public bool IsSend { get; set; } = false;
        public int NumberOfTry { get; set; } = 0;
    }
}
