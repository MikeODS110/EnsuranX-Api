using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class Provider : AuditableEntity<long>
    {
        public string Name { get; set; }
        public string Details { get; set; }
    }
}
