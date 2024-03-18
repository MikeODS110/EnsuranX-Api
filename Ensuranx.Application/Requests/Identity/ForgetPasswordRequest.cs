using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Identity
{
    public class ForgetPasswordOtpRequest
    {
        [Required(ErrorMessage = Constants.APIErrorMessages.EMAIL_REQUIRED)]
        public required string Email { get; set; }
  
        
    }
    public class ForgetPasswordRequest
    {
        [Required(ErrorMessage = Constants.APIErrorMessages.OTP_REQUIRED)]
        public required string Otp { get; set; }
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.PASSWORD_CONFIRM_REQUIRED)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = Constants.APIErrorMessages.PASSWORD_CONFIRM_DONOT_MATCH)]
        public required string ConfirmPassword { get; set; }
    }
}
