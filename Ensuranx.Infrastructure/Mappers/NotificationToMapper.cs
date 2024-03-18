using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class NotificationToMapper
    {
        public NotificationTo SetDataInNotificationToWithnotificationIdAndUserInfo(long notificationId, UserInfo userInfo)
        {
            NotificationTo notificationsTo = new NotificationTo();

            notificationsTo.NotificationsId = notificationId;
            notificationsTo.UserInfoId = userInfo.Id;
            notificationsTo.Email = "";
            notificationsTo.CreatedBy = "System";
            notificationsTo.LastModifiedBy = "System";
            notificationsTo.CreatedDateTime = DateTime.UtcNow;
            notificationsTo.LastModifiedDateTime = DateTime.UtcNow;

            return notificationsTo;
        }
    }
}
