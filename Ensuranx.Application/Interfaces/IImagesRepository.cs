using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IImagesRepository
    {
        public Task<List<Ensuranx.Domain.Entities.Images>> GetAllMedia(BasicFilter basicFilter,long userId);
        public Task<int> GetAllMediaCount(long userId);
        public Task<List<Images>> GetImageByTableIdAndConnectionEntity(long id,long connectionEntityId);
        public Task<List<Images>> GetUserProfileImagesByUserIdList(List<long> userInfoIdList);
        public Task<List<Images>> GetImageByUserIdList(List<long> userIdList);
        public Task<List<Images>> UpdateImagesAsync(List<Images> images);
        public Task<Images> UpdateImageAsync(Images images);
        public Task<Images> CreateImageAsync(Images images);
        public Task<List<Images>> CreateImagesAsync(List<Images> images);
        public IQueryable<Images> GetUserImagesForNotificationList(IQueryable<long> userInfoIdList);
        public IQueryable<Images> GetActivityImagesForNotificationList(IQueryable<long> activityIdList);
    }
}
