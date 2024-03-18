namespace Ensuranx.Application.Requests.Identity;

public class ValidateUserDetails
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
