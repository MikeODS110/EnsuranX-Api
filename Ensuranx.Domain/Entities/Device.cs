using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities
{
    public class Device: AuditableEntity<long>
    {
        public string DeviceToken { get; set; } 
    }
}
