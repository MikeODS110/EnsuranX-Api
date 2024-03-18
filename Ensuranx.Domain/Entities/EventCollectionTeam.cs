using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class EventCollectionTeam : AuditableEntity<long>
    {
        public virtual long EventCollectionId { get; set; }
        [ForeignKey("EventCollectionId")]
        public virtual EventCollection EventCollection { get; set; }

        public virtual long TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
        public string Sequence { get; set; } = "A";
    }
}
