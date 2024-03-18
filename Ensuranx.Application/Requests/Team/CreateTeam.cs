using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Team
{
    public class CreateTeam
    {
        [Required(ErrorMessage = Common.Constants.Constants.APIErrorMessages.NAME_REQUIRED)]
        public string Name { get; set; }
        [Required(ErrorMessage = Common.Constants.Constants.APIErrorMessages.ACTIVITIES_REQUIRED)]
        public long ActivityId { get; set; }
    }
}
