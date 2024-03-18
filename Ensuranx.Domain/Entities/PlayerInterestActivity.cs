using Ensuranx.Domain.Contracts;
using Ensuranx.Domain.IdentityExtensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class PlayerInterestActivity : AuditableEntity<long>
    {
        public virtual long? UserInfoId { get; set; }
        [ForeignKey("UserInfoId")]
        public virtual UserInfo UserInfo { get; set; }

        public virtual long ActivityCategoryId { get; set; }
        [ForeignKey("ActivityCategoryId")]
        public virtual ActivityCategory ActivityCategory { get; set; }
    }
}
