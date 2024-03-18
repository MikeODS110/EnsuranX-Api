using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Domain.Entities
{
    public class BranchScheduling : AuditableEntity<long>
    {
        public TimeSpan StartTime { get; set; } 
        public TimeSpan EndTime { get; set; } 
        public WeekDays WeekDay { get; set; }
        public virtual long BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
    }
}
