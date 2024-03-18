using Ensuranx.Domain.Contracts;
using Ensuranx.Domain.IdentityExtensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class UserInfo : AuditableEntity<long>
    {
        public string Name { get; set; }
        public virtual int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }
        public int OTPCode { get; set; }
        public DateTime DateOfRegisteration { get; set; }
        public bool IsSend { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public int ZipCode { get; set; }
        public bool EnableNotifications { get; set; } = true;


    }
}
