using ErrorOr;
using Ensuranx.Application.Contracts.Device;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Response.Other;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Services.Device
{
    public class DeviceService : IDevice
    {

        private IUnitOfWork<long> _iunitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
   
        private readonly IDevice _ideviceSerivce;
        private readonly IRoleRepository _iroleRepository;
       

        public DeviceService(ApplicationDbContext context, ILogger logger)
        {
            _logger = logger;
            _context = context;
       
            _iroleRepository = new RoleRepository(context);
            _iunitOfWork = new UnitOfWork<long>(context);
       
        }
     
     
            
        


       
    }
}
