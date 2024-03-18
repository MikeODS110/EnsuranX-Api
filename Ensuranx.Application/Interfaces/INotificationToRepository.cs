namespace Ensuranx.Application.Interfaces
{
    public interface INotificationToRepository
    {
        public Task<List<Domain.Entities.NotificationTo>> GetMobileToListByIsSendFalseAsync();
        public List<Domain.Entities.NotificationTo> GetMobileToListByIsSendFalse();

        public int GetCountOfUnseenNotificationsTo(long UserInfoId);
    }
}
