using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Event : AuditableEntity<long>
    {
        public virtual long? StatKeeperId { get; set; }
        [ForeignKey("StatKeeperId")]
        public virtual UserInfo UserInfo { get; set; }
        public string Name { get; set; }
        public DateTime EventDateTime { get; set; }
        public DateTime EventEndDateTime { get; set; }
        public bool IsEventStart { get; set; } = false;
    }
}
