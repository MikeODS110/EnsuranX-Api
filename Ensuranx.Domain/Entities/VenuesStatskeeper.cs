using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class VenuesStatskeeper : AuditableEntity<long>
    {
        public virtual long VenueId { get; set; }
        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }
        public virtual long? UserInfoId { get; set; }
        [ForeignKey("UserInfoId")]
        public virtual UserInfo UserInfo { get; set; }
    }
}
