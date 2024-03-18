using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class PaymentOption : AuditableEntity<long>
    {
        public string Name { get; set; }
    }
}
