using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities;

public class Media : AuditableEntity<long>
{
    public virtual long ActivityId { get; set; }
    [ForeignKey("ActivityId")]
    public required virtual Activity Activity { get; set; }

    public virtual long VenueId { get; set; }
    [ForeignKey("VenueId")]
    public required virtual Venue Venue { get; set; }

    public string Path { get; set; } = default!;
    public string Tag { get; set; } = default!;
}
