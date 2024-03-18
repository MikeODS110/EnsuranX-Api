using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Branch : AuditableEntity<long>
    {
        public required string Name { get; set; }
        public virtual long BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        public virtual long? UserInfoId { get; set; }
        [ForeignKey("UserInfoId")]
        public virtual UserInfo UserInfo { get; set; }
    }
}
