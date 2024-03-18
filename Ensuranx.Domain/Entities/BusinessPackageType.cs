using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class BusinessPackageType : AuditableEntity<long>
    {
        public string Name { get; set; }
    }
}
