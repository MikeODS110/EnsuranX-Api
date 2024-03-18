using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class EventCollection : AuditableEntity<long>
    {
        public virtual long VenueId { get; set; }
        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }

        public virtual long ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; }

        public virtual long? StatKeeperId { get; set; }
        [ForeignKey("StatKeeperId")]
        public virtual UserInfo UserInfo { get; set; }

        public virtual long EventTypeId { get; set; }
        [ForeignKey("EventTypeId")]
        public virtual EventType EventType { get; set; }

        public bool IsLocked { get; set; }

        //public virtual long DeviceId { get; set; }
        //[ForeignKey("DeviceId")]
        //public virtual Device Device{ get; set; }
    }
}
