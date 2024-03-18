using Ensuranx.Application.Contracts.Providers;
using Ensuranx.Application.Interfaces;
using Ensuranx.Common.WrapperInterface;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Infrastructure.Services.Provider
{
    public class ProviderService : IProviderService
    {
        private readonly ILogger _logger;

        private readonly ApplicationDbContext _context;
        private IUnitOfWork<long> _iunitOfWork;
        public ProviderService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _context = context;
            _logger = logger;
        }
        public async Task<List<Ensuranx.Domain.Entities.Provider>> GetAllProvider()
        {
            return await _iunitOfWork.Repository<Ensuranx.Domain.Entities.Provider>().GetAllAsync();
        }
    }
}
