using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Activity : AuditableEntity<long>
    {
        public virtual long ActivityCategoryId { get; set; }
        [ForeignKey("ActivityCategoryId")]
        public required virtual ActivityCategory ActivityCategory { get; set; }
        public virtual long? EventTypeId { get; set; }
        [ForeignKey("EventTypeId")]
        public virtual EventType EventType { get; set; }
        public required string Name { get; set; }
        public int MinPlayerPerTeam { get; set; }
        public int MaxPlayerPerTeam { get; set; }
        public int? MinutesOfActivity { get; set; }
        public long Quarters { get; set; } = 0;
    }
}
