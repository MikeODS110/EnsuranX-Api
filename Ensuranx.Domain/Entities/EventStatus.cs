using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class EventStatus : AuditableEntity<long>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
