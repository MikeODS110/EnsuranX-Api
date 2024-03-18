using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities;

public class EventCollectionDevice : AuditableEntity<long>
{
    public virtual long EventCollectionId { get; set; }
    [ForeignKey("EventCollectionId")]
    public virtual EventCollection EventCollection { get; set; }

    public virtual long DeviceId { get; set; }
    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; }
}
