using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Profile
{
    public class AddProfileDetails
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImage { get; set; }
        public string CoverImage { get; set; }

        [RegularExpression(@"^(00|[0-9][0-9]|[1-9]\d{2})$", ErrorMessage = "JerseyNo must be in two or three digits with optional leading zeros.")]
        public string JerseyNo { get;set; } 
    }
}
