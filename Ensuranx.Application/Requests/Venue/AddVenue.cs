using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Venue
{
    public class AddVenue
    {
        [Required(ErrorMessage = Common.Constants.Constants.APIErrorMessages.BRANCH_REQUIRED)]
        public long BranchId { get; set; }
        [Required(ErrorMessage = Common.Constants.Constants.APIErrorMessages.NAME_REQUIRED)]
        public string Name { get; set; }
        public List<long> StatkeeperIdList { get; set; }
        public List<long> ActivityIdList { get; set; }
    }
}
