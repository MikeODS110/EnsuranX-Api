using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Statkeeper
{
    public class AddStatkeeper
    {
        [Required(ErrorMessage = Constants.APIErrorMessages.NAME_REQUIRED)]
        public required string Name { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.PHONENUMBER_REQUIRED)]
        public required string PhoneNumber { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.ZIPCODE_REQUIRED)]
        public required int ZipCode { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.EMAIL_REQUIRED)]
        [EmailAddress]
        public required string Email { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.PASSWORD_REQUIRED)]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.BRANCH_REQUIRED)]
        public required long BranchId { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.VENUE_REQUIRED)]
        public List<long> VenueIdList { get; set; }
    }
}
