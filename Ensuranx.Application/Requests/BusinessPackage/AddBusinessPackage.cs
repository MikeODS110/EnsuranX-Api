using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.BusinessPackage
{
    public class AddBusinessPackage
    {
        public long BusinessPackageId { get; set; }
        [Required(ErrorMessage = Constants.APIErrorMessages.NAME_REQUIRED)]
        public string BusinessName { get; set; }
        public long BusinessTypeId { get; set; }
    }
}
