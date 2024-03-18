using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Identity
{
    public class RegistrationRequest
    {
        [Required(ErrorMessage = Constants.APIErrorMessages.ROLE_REQUIRED)]
        public required string RoleName { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.NAME_REQUIRED)]
        public required string Name { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.USERNAME_REQUIRED)]
        public required string Username { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.EMAIL_REQUIRED)]
        public required string Email { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.ZIPCODE_REQUIRED)]
        public int ZipCode { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.PHONENUMBER_REQUIRED)]
        public required string PhoneNumber { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.PASSWORD_REQUIRED)]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.PASSWORD_CONFIRM_REQUIRED)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = Constants.APIErrorMessages.PASSWORD_CONFIRM_DONOT_MATCH)]
        public required string ConfirmPassword { get; set; }
    }
}
