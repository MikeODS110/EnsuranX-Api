using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class TournamentTeam : AuditableEntity<long>
    {
        public virtual long TeamId { get; set; }
        [ForeignKey("TeamId")]
        public required virtual Team Team { get; set; }
        public virtual long TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }
        public bool IsAccepted { get; set; }
    }
}
