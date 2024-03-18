using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class EventLog : AuditableEntity<long>
    {
        public virtual long TeamMemberId { get; set; }
        [ForeignKey("TeamMemberId")]
        public virtual TeamMember TeamMember { get; set; }

        public virtual long EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        public virtual long StatId { get; set; }
        [ForeignKey("StatId")]
        public virtual Stat Stat { get; set; }
        public bool IsSucessfull { get; set; }
        public bool IsFix { get; set; }
        public int IsPause { get; set; }
        public bool IsShotClock { get; set; } = false;
    }
}
