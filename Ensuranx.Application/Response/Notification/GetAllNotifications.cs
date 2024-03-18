using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Ensuranx.Application.Response.Notification;

public class GetAllNotifications
{
    public long NotificationToId { get; set; }
    public long UserId { get; set; } = 0;
    public string NotificationSubject { get; set; } = string.Empty;
    public string NotificationDetails { get; set; } = string.Empty;
    public string NotificationImage { get; set; } = string.Empty;
    public Domain.Enums.Enum.NotificationsTypes NotificationsTypes { get; set; } = Domain.Enums.Enum.NotificationsTypes.FollowRequest;
    public bool IsNotificationView { get; set; }
    public bool IsNotificationOpen { get; set; }
    public bool IsAccepted { get; set; } = true;
    public bool CanFollow { get; set; } = true;
    public string LongAgo { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }
    public long TableId { get; set; }
}
