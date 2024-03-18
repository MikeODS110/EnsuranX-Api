using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Follow : AuditableEntity<long>
    {
        public virtual long? FollowerId { get; set; }
        [ForeignKey("FollowerId")]
        public virtual UserInfo? UserInfo { get; set; }

        public virtual long? FolloweeId { get; set; }
        [ForeignKey("FolloweeId")]
        public virtual UserInfo? UserInfo2 { get; set; }
            
        public virtual int Status { get; set; }
    }
}
