using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class TournamentVenue : AuditableEntity<long>
    {
        public virtual long VenueId { get; set; }
        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }
        public virtual long TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }
    }
}
