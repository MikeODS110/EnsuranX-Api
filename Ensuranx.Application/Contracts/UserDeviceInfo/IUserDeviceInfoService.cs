namespace Ensuranx.Application.Contracts.UserDeviceInfo;

public interface IUserDeviceInfoService
{
    public Task<List<Domain.Entities.UserDeviceInfo>> GetUserDeviceInfoList(List<long> userInfoIdList);
}
