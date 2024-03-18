using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Repositories;

public class UserDeviceInfoRepository : IUserDeviceInfoRepository
{
    private readonly ApplicationDbContext _context;

    public UserDeviceInfoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDeviceInfo>> GetUserDeviceInfoList(List<long> userInfoIdList)
    {
        return await _context.UserDeviceInfo.Where(x => x.IsInUse == true && userInfoIdList.Contains(x.UserInfoId) && !string.IsNullOrEmpty(x.DeviceId) &&  !string.IsNullOrEmpty(x.DeviceFCMToken)).ToListAsync();
    }

    public async Task<UserDeviceInfo> GetActiveUserDeviceInfoWithPlatform(long userId, Domain.Enums.Enum.Platform platform,string deviceId)
    {
        return await _context.UserDeviceInfo.AsNoTracking().Where(x => x.UserInfoId == userId && x.IsInUse != false && x.Platform == platform && x.DeviceId == deviceId).FirstOrDefaultAsync();
    }

    public async Task<UserDeviceInfo> GetUserDeviceInfoActiveByUserId(long userId)
    {
        return await _context.UserDeviceInfo.AsNoTracking().Where(x => x.IsInUse != false && x.UserInfoId == userId).FirstOrDefaultAsync();
    }

    public async Task<List<UserDeviceInfo>> GetUserDeviceInfoListByDeviceId(string deviceId)
    {
        var result = await _context.UserDeviceInfo.Where(x => x.DeviceId == deviceId && x.IsInUse == true).ToListAsync();
        return result;
    }

    public async Task<UserDeviceInfo> GetUserDeviceInfoByUserIdAndDeviceIdAndIsIOSOrAndroid(long userId, string deviceId, Domain.Enums.Enum.Platform platform)
    {
        var result = await _context.UserDeviceInfo.Where(x => x.UserInfoId == userId && x.DeviceId == deviceId && x.Platform == platform).FirstOrDefaultAsync();
        return result;
    }
}
