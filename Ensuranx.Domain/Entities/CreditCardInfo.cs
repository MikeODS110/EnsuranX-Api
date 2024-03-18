using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class CreditCardInfo : AuditableEntity<long>
    {
        public string CardName { get; set; }
        public virtual long PaymentOptionId { get; set; }
        [ForeignKey("PaymentOptionId")]
        public virtual PaymentOption PaymentOption { get; set; }
        public string CardNumber { get; set; }
        public int Cvv { get; set; }
        public string ExpiryDate { get; set; }
        public string CardBrand { get; set; }
        public string LastFour { get; set; }
        public virtual long? UserInfoId { get; set; }
        [ForeignKey("UserInfoId")]
        public virtual UserInfo UserInfo { get; set; }
    }
}
