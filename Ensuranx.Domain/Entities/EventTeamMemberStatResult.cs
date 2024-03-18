using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities;

public class EventTeamMemberStatResult : AuditableEntity<long>
{
    public virtual long EventId { get; set; }
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; }

    public virtual long TeamMemberId { get; set; }
    [ForeignKey("TeamMemberId")]
    public virtual TeamMember TeamMember { get; set; }
    public long TotalPoint { get; set; }
    public long Assist { get; set; }
    public long Rebound { get; set; }
    public long Steal { get; set; }
    public long Block { get; set; }
    public decimal WinScore { get; set; }
    public decimal TotalScore { get; set; }
    public decimal MvpPerc { get; set; }
    public decimal WinPerc { get; set; }
    public bool IsMvp { get; set; } = false;
}
