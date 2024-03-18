using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class VenueActivity : AuditableEntity<long>
    {
        public virtual long VenueId { get; set; }
        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }
        public virtual long ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; }
        public string? Description { get; set; }
    }
}
