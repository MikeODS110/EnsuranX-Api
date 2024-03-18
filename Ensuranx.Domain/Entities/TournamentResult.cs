using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities;

public class TournamentResult : AuditableEntity<long>
{
    public virtual long TournamentId { get; set; }
    [ForeignKey("TournamentId")]
    public virtual Tournament Tournament { get; set; }

    public virtual long TeamId { get; set; }
    [ForeignKey("TeamId")]
    public virtual Team Team { get; set; }

    public virtual long PositionId { get; set; }
    [ForeignKey("PositionId")]
    public virtual Position Position { get; set; }
}
