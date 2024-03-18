using FirebaseAdmin.Auth;
using Ensuranx.Application.Interfaces;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories
{
    public class ImagesRepository : IImagesRepository
    {
        private readonly ApplicationDbContext _context;

        public ImagesRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
     

        public IQueryable<Images> GetTournamentActivitiesImages(IQueryable<long> activityIdList)
        {
            return _context.Image.AsNoTracking().Where(x => activityIdList.Contains(x.TableId) &&
                                                    x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower() &&
                                                    x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()).AsQueryable();
        }

        public IQueryable<Images> GetUserProfileImages(IQueryable<long> userIdList)
        {
            return _context.Image.AsNoTracking().Where(x => userIdList.Contains(x.TableId) &&
                                                    x.ConnectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                                    x.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()).AsQueryable(); 
        }

        public IQueryable<Images> GetUserImagesForNotificationList(IQueryable<long> userInfoIdList)
        {
            var result = (from connectionEntity in _context.ConnectionEntity.AsNoTracking()
                               join image in _context.Image.Where(x => userInfoIdList.Contains(x.TableId))
                               on connectionEntity.Id equals image.ConnectionEntityId
                               where connectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                               image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                               select image
                      ).AsQueryable();
            return result;
        }

        public IQueryable<Images> GetActivityImagesForNotificationList(IQueryable<long> activityIdList)
        {
            var result = (from connectionEntity in _context.ConnectionEntity.AsNoTracking()
                          join image in _context.Image.Where(x => activityIdList.Contains(x.TableId))
                          on connectionEntity.Id equals image.ConnectionEntityId
                          where connectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Activity.ToString().ToLower() &&
                          image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                          select image
                      ).AsQueryable();
            return result;
        }

        public async Task<List<Images>> GetUserProfileImagesByUserIdList(List<long> userInfoIdList)
        {
            var result = await (from connectionEntity in _context.ConnectionEntity.AsNoTracking()
                                join image in _context.Image.Where(x => userInfoIdList.Contains(x.TableId))
                                on connectionEntity.Id equals image.ConnectionEntityId
                                where connectionEntity.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower() &&
                                image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()
                                select image
                       ).ToListAsync();
            return result;
        }

 

      

     

   

        public async Task<List<Ensuranx.Domain.Entities.Images>> GetAllMedia(BasicFilter basicFilter,long userId)
        {
            var query = await (from connectionEntity in _context.ConnectionEntity.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                               join image in _context.Image.Where(x => x.TableId == userId).Include(x => x.ImageType)
                               on connectionEntity.Id equals image.ConnectionEntityId
                               where image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Media.ToString().ToLower()
                               select image
                          ).Skip((basicFilter.PageNumber - 1) * basicFilter.PageSize)
                            .Take(basicFilter.PageSize).ToListAsync();
            return query;
        }

        public async Task<int> GetAllMediaCount(long userId)
        {
            return await (from connectionEntity in _context.ConnectionEntity.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                          join image in _context.Image.Where(x => x.TableId == userId).Include(x => x.ImageType)
                          on connectionEntity.Id equals image.ConnectionEntityId
                          where image.ImageType.Name.ToLower() == Domain.Enums.Enum.ImageType.Media.ToString().ToLower()
                          select image
                          ).CountAsync();
        }

        public async Task<List<Images>> GetImageByTableIdAndConnectionEntity(long id, long connectionEntityId)
        {
            return  await _context.Image.AsNoTracking().Where(x => x.TableId == id && x.ConnectionEntityId == connectionEntityId).ToListAsync();
        }

        public async Task<List<Images>> GetImageByUserIdList(List<long> userIdList)
        {
            return await (from connectionEntity in _context.ConnectionEntity.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.User.ToString().ToLower())
                          join image in _context.Image.Where(x => userIdList.Contains(x.TableId)).Include(x => x.ImageType)
                          on connectionEntity.Id equals image.ConnectionEntityId
                          select image
                          ).ToListAsync();
        }

        public async Task<List<Images>> UpdateImagesAsync(List<Images> images)
        {
            _context.UpdateRange(images);
            _context.SaveChanges();
            return images;
        }

        public async Task<Images> UpdateImageAsync(Images images)
        {
            _context.Update(images);
            _context.SaveChanges();
            return images;
        }

        public async Task<Images> CreateImageAsync(Images images)
        {
            await _context.AddAsync(images);
            _context.SaveChanges();
            return images;
        }

        public async Task<List<Images>> CreateImagesAsync(List<Images> images)
        {
            await _context.AddRangeAsync(images);
            _context.SaveChanges();
            return images;
        }
    }
}
