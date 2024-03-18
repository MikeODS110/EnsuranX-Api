using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class ActivityFamily : AuditableEntity<long>
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
