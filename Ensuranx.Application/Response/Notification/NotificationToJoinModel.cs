namespace Ensuranx.Application.Response.Notification;

public class NotificationToJoinModel
{
    public long NotificationToId { get; set; }
    public long UserInfoId { get; set; }
    public string Token { get; set; } = string.Empty;
    public Domain.Enums.Enum.Platform Platform { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int unseenCount { get; set; }
}
