using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class ImageMapper
    {
        public Images MapImageForCreate(long tableId,string path,long connectionEntityId, long imageTypeId,string email)
        {
            Images image = new Images();

            image.TableId = tableId;
            image.Path = path;
            image.ConnectionEntityId = connectionEntityId;
            image.ImageTypeId = imageTypeId;
            image.CreatedBy = email;
            image.LastModifiedBy = email;
            image.LastModifiedDateTime = DateTime.UtcNow;
            image.CreatedDateTime = DateTime.UtcNow;

            return image;
        }
    }
}
