namespace Ensuranx.Application.Response.Device;

public class ManageDevice
{
    public long EventCollectionId { get; set; }
    public long ActiveEventId { get; set; } = 0;
    public string Message { get; set; } = string.Empty;
}
