using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class BusinessPackages : AuditableEntity<long>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public virtual long BusinessTypeId { get; set; }
        [ForeignKey("BusinessTypeId")]
        public virtual BusinessType BusinessType { get; set; }
        public virtual long BusinessPackageTypeId { get; set; }
        [ForeignKey("BusinessPackageTypeId")]
        public virtual BusinessPackageType BusinessPackageType { get; set; }
    }
}
