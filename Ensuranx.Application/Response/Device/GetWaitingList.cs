namespace Ensuranx.Application.Response.Device;

public class GetWaitingList
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sequence { get; set; } = "01";
}
