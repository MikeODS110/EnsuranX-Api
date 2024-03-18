using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Identity
{
    public class LoginRequest
    {
        [Required(ErrorMessage = Constants.APIErrorMessages.EMAIL_REQUIRED)]
        [EmailAddress]
        public required string Email { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.PASSWORD_REQUIRED)]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceFCMToken { get; set; } = string.Empty;
        public Domain.Enums.Enum.Platform Platform { get; set; } = Domain.Enums.Enum.Platform.Android;
    }
}
