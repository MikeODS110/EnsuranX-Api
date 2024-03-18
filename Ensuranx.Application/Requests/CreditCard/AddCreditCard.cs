using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.CreditCard
{
    public class AddCreditCard
    {
        [Required(ErrorMessage = Constants.APIErrorMessages.NAME_REQUIRED)]
        public string CardName { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.CREDITCARD_REQUIRED)]
        public string CardNumber { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.CVV_REQUIRED)]
        public int Cvv { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.EXPIRE_DATE_REQUIRED)]
        public string ExpiryDate { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.PAYMENT_OPTION_REQUIRED)]
        public string PaymentOptionStr { get; set; }
    }
}
