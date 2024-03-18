using ErrorOr;
using Ensuranx.Application.Requests.BusinessPackage;
using Ensuranx.Application.Response.Other;

namespace Ensuranx.Application.Contracts.Business
{
    public interface IBusinessService
    {
        public Task<ErrorOr<GenericMessage>> AddBusinessPackageAsync(AddBusinessPackage addBusinessPackage, string email, long userId);
    }
}
