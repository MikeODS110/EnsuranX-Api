using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Stat : AuditableEntity<long>
    {
        public virtual long StatTypeId { get; set; }
        [ForeignKey("StatTypeId")]
        public virtual StatType StatType { get; set; }

        public virtual long ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; }

        public string Name { get; set; }
        public decimal MvpValue { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
    }
}
