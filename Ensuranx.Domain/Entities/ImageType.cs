using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class ImageType : AuditableEntity<long>
    {
        public string Name { get; set; }
    }
}
