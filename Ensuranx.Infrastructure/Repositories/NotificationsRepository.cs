using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Notification;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Common.Constant;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Repositories
{
    public class NotificationsRepository : INotificationsRepository
    {
        private readonly ApplicationDbContext _context;
        public NotificationsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notifications>> GetNotificationsListMobileAsync(List<long> notificationIdList)
        {
            return await _context.Notification.Where(x => x.NotificationPlatform == Domain.Enums.Enum.NotificationPlatform.Mobile && notificationIdList.Contains(x.Id) && x.NotificationSubject != null && x.NotificationSubject != string.Empty && x.NotificationDetail != null).ToListAsync();
        }

        public IQueryable<NotificationTo> GetAllNotificationToWithUserId(long userId)
        {
            return _context.NotificationTo.AsNoTracking().Where(x => x.UserInfoId == userId).AsQueryable();
        }

        public async Task<NotificationTo> GetNotificationByTableIdAndUserInfoId(long userId, long tableId)
        {
            return await _context.NotificationTo.AsNoTracking().Where(x => x.TableId == tableId && x.UserInfoId == userId).FirstOrDefaultAsync();
        }

        public async Task<List<GetAllNotifications>> GetAllNotification(long userId, BasicFilter basicFilter)
        {
            var results = await (from notificationTo in _context.NotificationTo.AsNoTracking().Where(x => x.UserInfoId == userId)
                                 join notification in _context.Notification.AsNoTracking()
                                 on notificationTo.NotificationsId equals notification.Id

                                 join userImageTbl in _context.Image.AsNoTracking().Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                 x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                 on notificationTo.TableId equals userImageTbl.TableId into newFullUserRightImage
                                 from userImageTblRightFull in newFullUserRightImage.DefaultIfEmpty()

                                 join followTbl in _context.Follow.AsNoTracking().Where(x => x.FolloweeId == userId)
                                 on notificationTo.TableId equals followTbl.FollowerId into newFullUserRightFollowee
                                 from followTblFullRight in newFullUserRightFollowee.DefaultIfEmpty()

                                 select new GetAllNotifications
                                 {
                                     UserId= (long)notificationTo.TableId,
                                     NotificationToId = notificationTo.Id,
                                     NotificationSubject = notification.NotificationSubject,
                                     NotificationDetails = notification.NotificationDetail,
                                     NotificationImage = userImageTblRightFull != null ? userImageTblRightFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     LongAgo = Ensuranx.Application.StaticFunction.Function.AsTimeAgo(notificationTo.CreatedDateTime),
                                     CreatedDateTime = notificationTo.CreatedDateTime,
                                     NotificationsTypes = notification.NotificationsType,
                                     IsNotificationView = notificationTo.IsView,
                                     IsNotificationOpen = notificationTo.IsOpen,
                                     IsAccepted = followTblFullRight != null && followTblFullRight.Status != 0 ? true : false,
                                     CanFollow = _context.Follow.AsNoTracking().Where(x => x.FollowerId == userId && x.FolloweeId == notificationTo.TableId).FirstOrDefault() != null ?
                                     false : true,
                                     TableId = userId,
                                     //UserId = followTblFullRight != null ? followTblFullRight.FollowerId.Value : 0,
                                 }).Distinct().OrderByDescending(x => x.NotificationToId).Skip((basicFilter.PageNumber - 1) * basicFilter.PageSize)
                                .Take(basicFilter.PageSize).ToListAsync();
            return results;
        }

        public async Task<int> GetAllNotificationCount(long userId)
        {
            var results = await (from notificationTo in _context.NotificationTo.AsNoTracking().Where(x => x.UserInfoId == userId)
                                 join notification in _context.Notification.AsNoTracking()
                                 on notificationTo.NotificationsId equals notification.Id

                                 join userImageTbl in _context.Image.AsNoTracking().Where(x => x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                 x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())
                                 on notificationTo.TableId equals userImageTbl.TableId into newFullUserRightImage
                                 from userImageTblRightFull in newFullUserRightImage.DefaultIfEmpty()

                                 join followTbl in _context.Follow.AsNoTracking().Where(x => x.FolloweeId == userId)
                                 on notificationTo.TableId equals followTbl.FollowerId into newFullUserRightFollowee
                                 from followTblFullRight in newFullUserRightFollowee.DefaultIfEmpty()

                                 select new GetAllNotifications
                                 {
                                     NotificationToId = notificationTo.Id,
                                     NotificationSubject = notification.NotificationSubject,
                                     NotificationDetails = notification.NotificationDetail,
                                     NotificationImage = userImageTblRightFull != null ? userImageTblRightFull.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE,
                                     LongAgo = Ensuranx.Application.StaticFunction.Function.AsTimeAgo(notificationTo.CreatedDateTime),
                                     CreatedDateTime = notificationTo.CreatedDateTime,
                                     NotificationsTypes = notification.NotificationsType,
                                     IsNotificationView = notificationTo.IsView,
                                     IsNotificationOpen = notificationTo.IsOpen,
                                     IsAccepted = followTblFullRight != null && followTblFullRight.Status != 0 ? true : false,
                                     CanFollow = _context.Follow.AsNoTracking().Where(x => x.FollowerId == userId && x.FolloweeId == notificationTo.TableId).FirstOrDefault() != null ?
                                     false : true,
                                     UserId = followTblFullRight != null ? followTblFullRight.FollowerId.Value : 0,
                                 }).Distinct().OrderByDescending(x => x.NotificationToId)
                                 .CountAsync();
            return results;
        }

        public async Task<int> GetAllNotificationSeenCount(long userId)
        {
            return await (from notificationTo in _context.NotificationTo.AsNoTracking().Where(x => x.UserInfoId == userId)
                          join notification in _context.Notification.AsNoTracking()
                          on notificationTo.NotificationsId equals notification.Id

                          where notificationTo.IsView != true

                          select new GetAllNotifications
                          {
                              NotificationToId = notificationTo.Id,
                          }).CountAsync();
        }

        public async Task<NotificationTo> UpdateNotificationTo(NotificationTo notificationTo)
        {
            _context.NotificationTo.Update(notificationTo);
            await _context.SaveChangesAsync();
            return notificationTo;
        }

        public async Task<bool> UpdateIsViewStatusForAllNotification(long userId)
        {
            var notificationToList = await (from notificationTo in _context.NotificationTo.AsNoTracking()
                                            .Where(x => x.UserInfoId == userId)
                                            join notification in _context.Notification.AsNoTracking()
                                            on notificationTo.NotificationsId equals notification.Id

                                            select notificationTo).ToListAsync();

            foreach (var notificationTo in notificationToList)
            {
                notificationTo.IsView = true;
                _context.NotificationTo.Update(notificationTo);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
