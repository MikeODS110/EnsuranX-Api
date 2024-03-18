using Ensuranx.Common;
using Ensuranx.Domain.Entities;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Mappers;

public class NotificationMapper
{
    public Notifications CreateNotification(GetNotificationMsg notificationMessage, NotificationPlatform notificationPlatform, NotificationsTypes notificationsType,string email)
    {
        Notifications notifications = new Notifications();

        
        notifications.NotificationSubject = notificationMessage.Title;
        notifications.NotificationDetail = notificationMessage.Body;
        notifications.NotificationsType = notificationsType;
        notifications.NotificationPlatform = notificationPlatform;
        notifications.CreatedBy = email;
        notifications.LastModifiedBy = email;
        notifications.CreatedDateTime = DateTime.UtcNow;
        notifications.LastModifiedDateTime = DateTime.UtcNow;

        return notifications;
    }

    public NotificationTo CreateNotificationTo(Notifications notify, long userId,string email,long tableId)
    {
        NotificationTo notifyTo = new NotificationTo();

        notifyTo.NotificationsId = notify.Id;
        notifyTo.UserInfoId = userId;
        notifyTo.Email = string.Empty;
        notifyTo.IsView = false;
        notifyTo.IsSend = false;
        notifyTo.TableId = tableId;
        notifyTo.CreatedBy = email;
        notifyTo.LastModifiedBy = email;
        notifyTo.CreatedDateTime = DateTime.UtcNow;
        notifyTo.LastModifiedDateTime = DateTime.UtcNow;

        return notifyTo;
    }
}
