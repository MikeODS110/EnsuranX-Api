using Ensuranx.Common.WrapperInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Application.Contracts.Providers
{
    public interface IProviderService 
    {
        public Task<List<Ensuranx.Domain.Entities.Provider>> GetAllProvider();
    }
}
