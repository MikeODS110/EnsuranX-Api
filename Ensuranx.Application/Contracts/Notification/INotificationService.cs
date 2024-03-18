using ErrorOr;
using Ensuranx.Application.Response.Notification;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Contracts.Notification;

public interface INotificationService
{
    Task<Notifications> CreateNotification(Notifications notification);
    // Task<NotificationTo> CreateNotificationTo(NotificationTo notificationTo);
    //Task<Domain.Entities.NotificationTo> CreateNotificationTo(Domain.Entities.NotificationTo notifyTo);
    public Task<List<Notifications>> GetNotificationsListMobileAsync(List<long> notificationIdList);
    public Task<ErrorOr<PagedResponse<List<GetAllNotifications>>>> GetAllNotificationList(long userId, BasicFilter basicFilter);
    public Task<ErrorOr<GenericMessage>> UpdateReadStatusAsync(long userId, string email, long notificationToId);
    public Task<ErrorOr<NotificationCount>> NotificationCountAsync(long userId, string email);
}
