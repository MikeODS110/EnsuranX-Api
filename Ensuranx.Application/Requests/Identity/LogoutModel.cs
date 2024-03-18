namespace Ensuranx.Application.Requests.Identity
{
    public class LogoutModel
    {
        public string DeviceId { get; set; }
        public Domain.Enums.Enum.Platform Platform { get; set; } = Domain.Enums.Enum.Platform.Android;
    }
}
