using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Profile
{
    public class UpdatePassword
    {
        [Required(ErrorMessage = Constants.APIErrorMessages.OLD_PASSWORD_REQUIRED)]
        public required string OldPassword { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.NEW_PASSWORD_REQUIRED)]
        public required string NewPassword { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.CONFIRM_PASSWORD_REQUIRED)]
        [Compare("NewPassword", ErrorMessage = Constants.APIErrorMessages.PASSWORD_CONFIRM_DONOT_MATCH)]
        public required string ConfirmPassword { get; set; }
    }
}
