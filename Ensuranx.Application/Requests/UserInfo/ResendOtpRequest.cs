namespace Ensuranx.Application.Requests.UserInfo
{
    public class ResendOtpRequest
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
