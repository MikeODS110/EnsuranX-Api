using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class TeamMember : AuditableEntity<long>
    {
        public virtual long TeamId { get; set; }
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        public virtual long TeamMemberRoleId { get; set; }
        [ForeignKey("TeamMemberRoleId")]
        public virtual TeamMemberRole TeamMemberRole { get; set; }
        public virtual long? PlayerId { get; set; }
        [ForeignKey("PlayerId")]
        public virtual UserInfo UserInfo { get; set; }
        public bool IsAccepted { get; set; } = false;
        public bool IsBan { get; set; } = false;
        public DateTime IsAcceptedDateTime { get; set; }
        public DateTime LeftDateTime { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsSubstituteOrDisqualify { get; set; } = false;
        public bool IsJoinedCollection { get; set; } = false;
        public string JerseyNo { get; set; } = "0";
    }
}
