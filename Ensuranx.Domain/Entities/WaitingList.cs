using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class WaitingList : AuditableEntity<long>
    {
        public virtual long EventCollectionId { get; set; }
        [ForeignKey("EventCollectionId")]
        public virtual EventCollection EventCollection { get; set; }
        public virtual long UserInfoId { get; set; }
        [ForeignKey("UserInfoId")]
        public virtual UserInfo UserInfo { get; set; }

        public int PlayerNumber { get; set; } = 0;

    }
}
