namespace Ensuranx.Application.Response.User;

public class ValidateUserDetailResponse
{
    public bool Email { get; set; } = true;
    public bool UserName { get; set; } = true;
    public bool PhoneNumber { get; set; } = true;
}
