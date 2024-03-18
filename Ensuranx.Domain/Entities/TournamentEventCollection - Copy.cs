using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class TournamentEventCollection : AuditableEntity<long>
    {
        public virtual long EventCollectionId { get; set; }
        [ForeignKey("EventCollectionId")]
        public virtual EventCollection EventCollection { get; set; }

        public virtual long TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }
    }
}
