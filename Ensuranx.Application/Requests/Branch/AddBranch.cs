using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Branch
{
    public class AddBranch
    {
        [Required(ErrorMessage = Common.Constants.Constants.APIErrorMessages.NAME_REQUIRED)]
        public string Name { get; set; }
        [Required(ErrorMessage = Common.Constants.Constants.APIErrorMessages.EMAIL_REQUIRED)]
        public string Email { get; set; }
        [Required(ErrorMessage = Common.Constants.Constants.APIErrorMessages.PHONENUMBER_REQUIRED)]
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        [Required(ErrorMessage = Common.Constants.Constants.APIErrorMessages.ADDRESS_REQUIRED)]
        public string Address { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        [Required(ErrorMessage = Common.Constants.Constants.APIErrorMessages.BUSINESS_REQUIRED)]
        public long BusinessId { get; set; }
    }
}
