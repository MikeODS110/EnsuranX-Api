using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class PermissionType : AuditableEntity<long>
    {
        public string Name { get; set; }
    }
}
