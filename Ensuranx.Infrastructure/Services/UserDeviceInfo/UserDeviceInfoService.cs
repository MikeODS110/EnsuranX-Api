using Ensuranx.Application.Contracts.UserDeviceInfo;
using Ensuranx.Application.Interfaces;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Serilog;

namespace Ensuranx.Infrastructure.Services.UserDeviceInfo;

public class UserDeviceInfoService : IUserDeviceInfoService
{
    private IUnitOfWork<long> _iunitOfWork;
    private readonly ILogger _logger;
    private readonly ApplicationDbContext _context;

    public UserDeviceInfoService(ApplicationDbContext context, ILogger logger)
    {
        _iunitOfWork = new UnitOfWork<long>(context);
        _logger = logger;
        _context = context;
    }

    public async Task<List<Domain.Entities.UserDeviceInfo>> GetUserDeviceInfoList(List<long> userInfoIdList)
    {
        return await _iuserDeviceInfoRepository.GetUserDeviceInfoList(userInfoIdList);
    }
}
