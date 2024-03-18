using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class ActivityCategory : AuditableEntity<long>
    {
        public virtual long ActivityFamilyId { get; set; }
        [ForeignKey("ActivityFamilyId")]
        public required virtual ActivityFamily ActivityFamily { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsTimeBased { get; set; } = true;
    }
}
