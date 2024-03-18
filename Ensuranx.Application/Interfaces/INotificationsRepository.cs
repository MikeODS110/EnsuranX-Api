using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface INotificationsRepository
    {
        public Task<List<Notifications>> GetNotificationsListMobileAsync(List<long> notificationIdList);
        public Task<NotificationTo> UpdateNotificationTo(NotificationTo notificationTo);
        public Task<bool> UpdateIsViewStatusForAllNotification(long userId);
        public Task<int> GetAllNotificationCount(long userId);
        public Task<int> GetAllNotificationSeenCount(long userId);
        public IQueryable<NotificationTo> GetAllNotificationToWithUserId(long userId);
        public Task<NotificationTo> GetNotificationByTableIdAndUserInfoId(long userId, long tableId);
    }
}
