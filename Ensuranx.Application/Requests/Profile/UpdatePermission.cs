namespace Ensuranx.Application.Requests.Profile;

public class UpdatePermission
{
    public PermissionUpdate ViewMyProfile { get; set; }
    public PermissionUpdate ViewMyLastSeen { get; set; }
    public PermissionUpdate ViewMyPlayingStatus { get; set; }
    public PermissionUpdate ViewMyLastPlayed { get; set; }
}

public class PermissionUpdate
{
    public long PermissionId { get; set; }
    public List<long> PermissionTypeIdList { get; set; }
}
