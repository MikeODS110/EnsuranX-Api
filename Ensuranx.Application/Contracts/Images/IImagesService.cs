using ErrorOr;
using Ensuranx.Application.Response.Image;
using Ensuranx.Common.PaginationResponse;

namespace Ensuranx.Application.Contracts.Images
{
    public interface IImagesService
    {
        public Task<List<Ensuranx.Domain.Entities.Images>> GetUserProfileImages(int userId);
        public Task<ErrorOr<PagedResponse<List<GetAllMedia>>>> GetAllMedia(BasicFilter basicFilter,long userId);
        public Task<Ensuranx.Domain.Entities.Images> GetUserProfileImage(int userId);
        public Task<Ensuranx.Domain.Entities.Images> GetUserCoverImage(int userId);
    }
}
