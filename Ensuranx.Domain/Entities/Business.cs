using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Business : AuditableEntity<long>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual long BusinessTypeId { get; set; }
        [ForeignKey("BusinessTypeId")]
        public virtual BusinessType BusinessType { get; set; }
        public virtual long BusinessOwnerId { get; set; }
        [ForeignKey("BusinessOwnerId")]
        public virtual UserInfo UserInfo { get; set; }
    }
}
