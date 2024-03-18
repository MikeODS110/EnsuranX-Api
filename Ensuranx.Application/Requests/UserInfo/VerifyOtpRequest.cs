using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.UserInfo
{
    public class VerifyOtpRequest
    {
        [Required(ErrorMessage = Constants.APIErrorMessages.OTP_REQUIRED)]
        public int OtpCode { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.EMAIL_REQUIRED)]
        public string Email { get; set; }
    }
}
