using Ensuranx.Domain.Contracts;
using Ensuranx.Domain.IdentityExtensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class TournamentStatkeeper : AuditableEntity<long>
    {
        public virtual long? StatkeeperId { get; set; }
        [ForeignKey("StatkeeperId")]
        public virtual UserInfo UserInfo { get; set; }
        public virtual long TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }
    }
}
