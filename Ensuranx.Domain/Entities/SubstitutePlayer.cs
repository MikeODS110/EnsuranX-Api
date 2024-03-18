using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class SubstitutePlayer: AuditableEntity<long>
    {
        public virtual long EventId { get; set; }
        public virtual long TeamMemberId { get; set; }
        public long SubstitutedPlayer { get; set; }
        public long SubstituteToPlayer { get; set; }
        public bool IsDisqualify { get; set; }
        public string Description { get; set; }
    }
}
