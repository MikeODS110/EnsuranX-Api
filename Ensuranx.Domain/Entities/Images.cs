using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Images : AuditableEntity<long>
    {
        public long TableId { get; set; }
        public virtual long ImageTypeId { get; set; }
        [ForeignKey("ImageTypeId")]
        public virtual ImageType ImageType { get; set; }
        public virtual long ConnectionEntityId { get; set; }
        [ForeignKey("ConnectionEntityId")]
        public virtual ConnectionEntity ConnectionEntity { get; set; }
        public string Path { get; set; }
    }
}
