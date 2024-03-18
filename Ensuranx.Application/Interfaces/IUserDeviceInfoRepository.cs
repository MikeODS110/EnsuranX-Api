using Ensuranx.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Application.Interfaces;

public interface IUserDeviceInfoRepository
{
    public Task<List<UserDeviceInfo>> GetUserDeviceInfoList(List<long> userInfoIdList);
    public Task<UserDeviceInfo> GetActiveUserDeviceInfoWithPlatform(long userId, Domain.Enums.Enum.Platform Platform,string deviceId);

    public Task<UserDeviceInfo> GetUserDeviceInfoActiveByUserId(long userId);

    public Task<List<UserDeviceInfo>> GetUserDeviceInfoListByDeviceId(string deviceId);

    public Task<UserDeviceInfo> GetUserDeviceInfoByUserIdAndDeviceIdAndIsIOSOrAndroid(long userId, string deviceId, Domain.Enums.Enum.Platform platform);
}
