using ErrorOr;
using Ensuranx.Application.Contracts.Images;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Image;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Ensuranx.Infrastructure.Services.Images
{
    public class ImagesService : IImagesService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly IImagesRepository _iimagesRepository;
        private readonly ILogger _logger;

        public ImagesService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _iimagesRepository = new ImagesRepository(context);
        }
        public async Task<List<Ensuranx.Domain.Entities.Images>> GetUserProfileImages(int userId)
        {
            return await _iimagesRepository.GetUserProfileImages(userId);
        }

        public async Task<Ensuranx.Domain.Entities.Images> GetUserProfileImage(int userId)
        {
            return await _iimagesRepository.GetUserProfileImage(userId);
        }
        public async Task<Ensuranx.Domain.Entities.Images> GetUserCoverImage(int userId)
        {
            return await _iimagesRepository.GetUserCoverImage(userId);
        }

        public async Task<ErrorOr<PagedResponse<List<GetAllMedia>>>> GetAllMedia(BasicFilter basicFilter,long userId)
        {
            try
            {
                List<Ensuranx.Domain.Entities.Images> imageList = await _iimagesRepository.GetAllMedia(basicFilter,userId);
                int TotalRecords = await _iimagesRepository.GetAllMediaCount(userId);
                List<GetAllMedia> getAllMediaList = imageList.Adapt<List<GetAllMedia>>();

                _logger.LogInformation("Media successfully fetch {@getAllMediaList}", getAllMediaList);

                return new PagedResponse<List<GetAllMedia>>(TotalRecords, getAllMediaList, basicFilter.PageNumber, basicFilter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
    }
}
