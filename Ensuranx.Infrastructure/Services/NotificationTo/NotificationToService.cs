

using Ensuranx.Application.Contracts.NotificationTo;
using Ensuranx.Application.Interfaces;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;

namespace Ensuranx.Infrastructure.Services.NotificationTo
{
    public class NotificationToService : INotificationToService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly INotificationToRepository _inotificationToRepository;
        private readonly ApplicationDbContext _context;
        public NotificationToService(ApplicationDbContext context)
        {
            _context = context;
            _iunitOfWork = new UnitOfWork<long>(context);
            _inotificationToRepository = new NotificationToRepository(context);
        }

        public async Task<Domain.Entities.NotificationTo> CreateNotificationTo(Domain.Entities.NotificationTo notificationTo)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            var result = await _iunitOfWork.Repository<Domain.Entities.NotificationTo>().AddAsync(notificationTo);
            await _iunitOfWork.Commit(cancellationToken);
            return result;
        }

        public async Task<List<Domain.Entities.NotificationTo>> GetMobileToListByIsSendFalseAsync()
        {
            return await _inotificationToRepository.GetMobileToListByIsSendFalseAsync();
        }
        
        public List<Domain.Entities.NotificationTo> GetMobileToListByIsSendFalse()
        {
            return _inotificationToRepository.GetMobileToListByIsSendFalse();
        }

        public async Task<Domain.Entities.NotificationTo> UpdateNotificationTo(Domain.Entities.NotificationTo notificationTo)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            await _iunitOfWork.Repository<Domain.Entities.NotificationTo>().UpdateAsync(notificationTo);
            await _iunitOfWork.Commit(cancellationToken);
            return notificationTo;
        }

        public Domain.Entities.NotificationTo UpdateNotificationToWithoutAsync(Domain.Entities.NotificationTo notificationTo)
        {
            _context.NotificationTo.Update(notificationTo);
            _context.SaveChanges();

            return notificationTo;
        }

        public int GetCountOfUnseenNotificationsTo(long UserInfoId)
        {
            return _inotificationToRepository.GetCountOfUnseenNotificationsTo(UserInfoId);
        }
    }
}
