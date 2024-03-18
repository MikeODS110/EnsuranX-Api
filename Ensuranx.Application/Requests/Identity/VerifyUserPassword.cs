using Ensuranx.Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Ensuranx.Application.Requests.Identity;

public class VerifyUserPassword
{
    [Required(ErrorMessage = Constants.APIErrorMessages.PASSWORD_REQUIRED)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;    
}

public class ApiArgumetns
{
    public string ZIP {  get; set; }
    public string  APIKey {  get; set; }
}