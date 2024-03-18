using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Profile
{
    public class UserInterestsActivities
    {
        [Required(ErrorMessage = Constants.APIErrorMessages.ACTIVITIES_REQUIRED)]
        public List<long> ActivityCategoryIdList { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.ZIPCODE_REQUIRED)]
        public int ZipCode { get; set; }
    }
}
