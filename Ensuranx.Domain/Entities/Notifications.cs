using Ensuranx.Domain.Contracts;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Domain.Entities
{
    public class Notifications : AuditableEntity<long>
    {
        public string? NotificationSubject { get; set; }
        public string? NotificationDetail { get; set; }
        public NotificationsTypes NotificationsType { get; set; }
        public NotificationPlatform NotificationPlatform { get; set; }
    }
}
