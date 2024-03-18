namespace Ensuranx.Application.Response.User;

public class PermissionResponse
{
    public List<PermissionDetail> PermissionDetailList { get; set; }
}

public class PermissionDetail
{
    public long PermissionId { get; set; }
    public string PermissionName { get; set; } = default!;
    public List<Dropdown> PermissionList { get; set; }
}

public class Dropdown
{
    public long Value { get; set; }
    public string Text { get; set; }
    public bool Selected { get; set; }
}

