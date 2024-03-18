using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Team : AuditableEntity<long>
    {
        public string Name { get; set; }
        public virtual long TeamTypeId { get; set; }
        [ForeignKey("TeamTypeId")]
        public virtual TeamType TeamType { get; set; }
        public string Colour { get; set; }
    }
}
