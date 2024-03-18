using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class ConnectionEntity : AuditableEntity<long>
    {
        public string Name { get; set; }
    }
}
