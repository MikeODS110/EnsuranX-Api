using Ensuranx.Application.Interfaces;
using Ensuranx.Common.Constants;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories;

public class NotificationToRepository : INotificationToRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationToRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Domain.Entities.NotificationTo>> GetMobileToListByIsSendFalseAsync()
    {
        List<Domain.Entities.NotificationTo> notificationToList = new List<Domain.Entities.NotificationTo>();

        notificationToList = await _context.NotificationTo
                                        .Where(x => x.IsSend == false && x.Notifications.NotificationPlatform == Domain.Enums.Enum.NotificationPlatform.Mobile)
                                        .OrderBy(x => x.NumberOfTry).ThenBy(x => x.Id).Take(StaticJobsConstants.NumberOfReocrdToFetch).ToListAsync();
        return notificationToList;
    }
    
    public List<Domain.Entities.NotificationTo> GetMobileToListByIsSendFalse()
    {
        List<Domain.Entities.NotificationTo> notificationToList = new List<Domain.Entities.NotificationTo>();

        notificationToList = _context.NotificationTo
                                        .Where(x => x.IsSend == false && x.Notifications.NotificationPlatform == Domain.Enums.Enum.NotificationPlatform.Mobile)
                                        .OrderBy(x => x.NumberOfTry).ThenBy(x => x.Id).Take(StaticJobsConstants.NumberOfReocrdToFetch).ToList();
        return notificationToList;
    }

    public int GetCountOfUnseenNotificationsTo(long UserInfoId)
    {
        var query = _context.NotificationTo.AsNoTracking().Where(x => x.Notifications.NotificationPlatform == Domain.Enums.Enum.NotificationPlatform.Mobile && x.UserInfoId == UserInfoId && x.IsView == false).Count();
        return query;
    }
}
