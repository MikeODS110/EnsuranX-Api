using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class ActivityStat : AuditableEntity<long>
    {
        public virtual long ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public required virtual Activity Activity { get; set; }

        public virtual long StatId { get; set; }
        [ForeignKey("StatId")]
        public required virtual Stat Stats { get; set; }

        public string Value { get; set; }
        public string MvpCalculation { get; set; }
    }
}
