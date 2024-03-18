

using ErrorOr;
using FirebaseAdmin.Messaging;
using Ensuranx.Application.Contracts.Notification;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Notification;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ensuranx.Infrastructure.Services.Notification;

public class NotificationService : INotificationService
{
    private IUnitOfWork<long> _iunitOfWork;
    private readonly INotificationsRepository _inotificationRepository;
    private readonly IFollowRepository _ifollowRepository;
    private readonly IImagesRepository _imagesRepository;
    private readonly ILogger _logger;

    public NotificationService(ApplicationDbContext context, ILogger logger)
    {
        _iunitOfWork =  new UnitOfWork<long>(context);
        _inotificationRepository = new NotificationsRepository(context);
        _ifollowRepository = new FollowRepository(context);
        _imagesRepository = new ImagesRepository(context);  
        _logger = logger;
    }

    public async Task<Notifications> CreateNotification(Notifications notification)
    {
        CancellationToken cancellationToken = CancellationToken.None;
      
        var result = await _iunitOfWork.Repository<Notifications>().AddAsync(notification);
        await _iunitOfWork.Commit(cancellationToken);

        return result;
    }

    public async Task<List<Notifications>> GetNotificationsListMobileAsync(List<long> notificationIdList)
    {
        return await _inotificationRepository.GetNotificationsListMobileAsync(notificationIdList);
    }
    public async Task<List<Notifications>> GetAllNotication(Notifications notification)
    {
        List<Notifications> notifications = new List<Notifications>();
        CancellationToken cancellationToken = CancellationToken.None;
        var result = await _iunitOfWork.Repository<Notifications>().AddAsync(notification);
        await _iunitOfWork.Commit(cancellationToken);
        return notifications;
    }
    
    /// <summary>
    /// all notification list
    /// </summary>
    /// <param name="userId">user whi is requesting the endpoint</param>
    /// <returns></returns>
    public async Task<ErrorOr<PagedResponse<List<GetAllNotifications>>>> GetAllNotificationList(long userId, BasicFilter basicFilter)
    {
        try
        {
            //IQueryable<Domain.Entities.NotificationTo> iNotificationTo = _inotificationRepository.GetAllNotificationToWithUserId(userId);
            //IQueryable<Domain.Entities.NotificationTo> iFollowRequest = iNotificationTo.Where(x => x.Notifications.NotificationsType == Domain.Enums.Enum.NotificationsTypes.FollowRequest).AsQueryable();
            //IQueryable<Domain.Entities.NotificationTo> iTeamFollowRequest = iNotificationTo.Where(x => x.Notifications.NotificationsType == Domain.Enums.Enum.NotificationsTypes.TeamJoinRequest).AsQueryable();

            //IQueryable<Domain.Entities.Follow> iFollow = _ifollowRepository.GetAllFollowRequestForThisUserNotAcceptedYet(userId);
            //IQueryable<long> userInfoIdList = iFollow.Select(x => x.FollowerId.Value).AsQueryable();

            //IQueryable<Domain.Entities.Images> imageList = _imagesRepository.GetUserImagesForNotificationList(userInfoIdList);

            var statusIsUpdated = await _inotificationRepository.UpdateIsViewStatusForAllNotification(userId);
            var allNotificationList = await _inotificationRepository.GetAllNotification(userId, basicFilter);
            var allNotificationCount = await _inotificationRepository.GetAllNotificationCount(userId);

            _logger.LogInformation("allNotifications {@allNotificationList}", allNotificationList);
            return new PagedResponse<List<GetAllNotifications>>(allNotificationCount, allNotificationList, basicFilter.PageNumber, basicFilter.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception Occurred {@ex}", ex);
            return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
        }
       
    }

    /// <summary>
    /// update the is open status for each notification
    /// </summary>
    /// <param name="userId">user who is requesting the endpoint</param>
    /// <param name="email">user who is requesting the endpoint</param>
    /// <param name="notificationToId">notification which status needs to change</param>
    /// <returns></returns>
    public async Task<ErrorOr<GenericMessage>> UpdateReadStatusAsync(long userId, string email, long notificationToId)
    {
        CancellationToken cancellationToken = CancellationToken.None;
        try
        {
            Domain.Entities.NotificationTo notificationTo = await _iunitOfWork.Repository<Domain.Entities.NotificationTo>().GetByIdAsync(notificationToId);

            if (notificationTo is null)
            {
                _logger.LogWarning($"{Domain.Common.Errors.Errors.Notification.NoNotificationFound}");
                return Domain.Common.Errors.Errors.Notification.NoNotificationFound;
            }
            else
            {
                notificationTo.IsOpen = true;
                await _iunitOfWork.Repository<Domain.Entities.NotificationTo>().UpdateAsync(notificationTo);
                await _iunitOfWork.Commit(cancellationToken);

                _logger.LogInformation($"{Constants.APIErrorMessages.RECORD_UPDATED}");
                return new GenericMessage { Message = Constants.APIErrorMessages.RECORD_UPDATED };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception Occurred {@ex}", ex);
            return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
        }
    }


    /// <summary>
    /// notifications which are not seen yet
    /// </summary>
    /// <param name="userId">user who is requesting the endpoint</param>
    /// <param name="email">user who is requesting the endpoint</param>
    /// <returns></returns>
    public async Task<ErrorOr<NotificationCount>> NotificationCountAsync(long userId, string email)
    {
        try
        {
            int NotificationBadgeCount = await _inotificationRepository.GetAllNotificationSeenCount(userId);

            _logger.LogInformation($"Notification count: {NotificationBadgeCount}");
            return new NotificationCount { NotificationBadgeCount = NotificationBadgeCount };
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception Occurred {@ex}", ex);
            return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
        }
    }
}
