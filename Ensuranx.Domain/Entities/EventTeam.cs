using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class EventTeam : AuditableEntity<long>
    {
        public virtual long EventStatusId { get; set; }
        [ForeignKey("EventStatusId")]
        public virtual EventStatus EventStatus { get; set; }
        public virtual long EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
        public virtual long EventCollectionTeamId { get; set; }
        [ForeignKey("EventCollectionTeamId")]
        public virtual EventCollectionTeam EventCollectionTeam { get; set; }
        public string? Description { get; set; } = string.Empty;
        public long Points { get; set; } = 0;
        public int Round { get; set; } = 0;
        public char? Group { get; set; }
    }
}
