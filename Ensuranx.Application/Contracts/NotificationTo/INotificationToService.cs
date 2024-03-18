namespace Ensuranx.Application.Contracts.NotificationTo
{
    public interface INotificationToService
    {
        public Task<Ensuranx.Domain.Entities.NotificationTo> CreateNotificationTo(Ensuranx.Domain.Entities.NotificationTo notificationTo);

        public Task<List<Domain.Entities.NotificationTo>> GetMobileToListByIsSendFalseAsync();
        public List<Domain.Entities.NotificationTo> GetMobileToListByIsSendFalse();

        public Task<Domain.Entities.NotificationTo> UpdateNotificationTo(Domain.Entities.NotificationTo notificationTo);
        public int GetCountOfUnseenNotificationsTo(long UserInfoId);

        public Domain.Entities.NotificationTo UpdateNotificationToWithoutAsync(Domain.Entities.NotificationTo notificationTo);
    }
}
