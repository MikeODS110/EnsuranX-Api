using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Connection : AuditableEntity<long>
    {
        public virtual long ConnectionTypeId { get; set; }
        [ForeignKey("ConnectionTypeId")]
        public virtual ConnectionType ConnectionType { get; set; }
        public string? Name { get; set; }
        public string Value { get; set; }
        public string? Description { get; set; }
        public virtual long ConnectionEntityId { get; set; }
        [ForeignKey("ConnectionEntityId")]
        public virtual ConnectionEntity ConnectionEntity { get; set; }
        public long TableId { get; set; }
    }
}
