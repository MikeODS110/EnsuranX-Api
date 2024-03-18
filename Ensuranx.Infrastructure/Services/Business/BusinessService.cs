using ErrorOr;
using Ensuranx.Application.Contracts.Business;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.BusinessPackage;
using Ensuranx.Application.Response.BusinessPackage;
using Ensuranx.Application.Response.Other;
using Ensuranx.Common.Constants;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ensuranx.Infrastructure.Services.Business
{
    /// <summary>
    /// contains all actions related to business entity
    /// </summary>
    public class BusinessService : IBusinessService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;
        private readonly IBusinessRepository _ibusinessRepository;
        private readonly ApplicationDbContext _context;

        public BusinessService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _ibusinessRepository = new BusinessRepository(context);
            _context = context;
        }

        /// <summary>
        /// business packages details based on business types
        /// </summary>
        /// <param name="businessType">business type id</param>
        /// <returns>package name price description and payment options</returns>
        public async Task<ErrorOr<AllPackagesResponse>> GetBusinessPackagesAsync(long businessType)
        {
            _logger.LogInformation("GetBusinessPackagesAsync");
            BusinessMapper businessMapper = new BusinessMapper();

            try
            {
                var response = await _ibusinessRepository.GetBusinessPackagesAsync(businessType);
                var values = await _iunitOfWork.Repository<Ensuranx.Domain.Entities.PaymentOption>().GetAllAsync();

                AllPackagesResponse allPackagesResponse = businessMapper.MappingBusinessPackagesResponse(values, response);

                _logger.LogInformation("GetBusinessPackagesAsync : {@allPackagesResponse}", allPackagesResponse);

                return allPackagesResponse.PackageDetail.Any() ? allPackagesResponse : Domain.Common.Errors.Errors.User.NoPackagesFound;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }


        /// <summary>
        /// get all business type for dropdown
        /// </summary>
        /// <param name="userId">user who is requesting the endpoint</param>
        /// <param name="email">user who is requesting the endpoint</param>
        /// <returns></returns>
        public async Task<ErrorOr<List<GetBusinessType>>> GetBusinessTypeAsync(long userId, string email)
        {
            _logger.LogInformation("GetBusinessTypeAsync");

            try
            {
                var values = await _iunitOfWork.Repository<Ensuranx.Domain.Entities.BusinessType>().GetAllAsync();
                List<GetBusinessType> getBusinessTypeList = values.Adapt<List<GetBusinessType>>();

                _logger.LogInformation("getBusinessTypeList : {@getBusinessTypeList}", getBusinessTypeList);
                return getBusinessTypeList.Any() ? getBusinessTypeList : Domain.Common.Errors.Errors.User.NoBusinessTypeFound;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }


        /// <summary>
        /// assign business package to the user
        /// </summary>
        /// <param name="addBusinessPackage">model constains the id of the selected business package</param>
        /// <returns>problem statement in case of error and success message in case of success</returns>
        public async Task<ErrorOr<GenericMessage>> AddBusinessPackageAsync(AddBusinessPackage addBusinessPackage,string email,long userId)
        {
            _logger.LogInformation("AddBusinessPackageAsync");
            CancellationToken cancellationToken = CancellationToken.None;
            UserBusinessPackageMapper userBusinessPackageMapper = new UserBusinessPackageMapper();
            BusinessMapper businessMapper = new BusinessMapper();

            try
            {
                Domain.Entities.UserInfo userInfo = await _iunitOfWork.Repository<Ensuranx.Domain.Entities.UserInfo>().GetByIdAsync(userId);
                var userBusinessPackageDetail = await _ibusinessRepository.GetUserBusinessPackageByUserIdAsync(userId);
                var businessDetailsToUpdate = await _ibusinessRepository.GetBusinessByUserIdAsync(userId);

                if (userInfo is null)
                {
                    _logger.LogWarning("UserInfo not found");
                    return Domain.Common.Errors.Errors.User.ErrorUserNotFound;
                }
                else
                {
                    Domain.Entities.UserBusinessPackage userBusinessPackage = userBusinessPackageDetail is not null ? userBusinessPackageMapper.MapUserBusinessPackageToUpdate(email, userId, userBusinessPackageDetail, addBusinessPackage.BusinessPackageId)
                        : userBusinessPackageMapper.MapUserBusinessPackageToCreate(email, userId, addBusinessPackage.BusinessPackageId);

                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        if (userBusinessPackageDetail is not null)
                        {
                            await _iunitOfWork.Repository<Ensuranx.Domain.Entities.UserBusinessPackage>().UpdateAsync(userBusinessPackageDetail);
                            await _iunitOfWork.Commit(cancellationToken);

                            businessDetailsToUpdate = businessMapper.MappingBusinessEntityToUpdate(addBusinessPackage.BusinessName, userId, addBusinessPackage.BusinessTypeId, email, businessDetailsToUpdate);

                            await _iunitOfWork.Repository<Ensuranx.Domain.Entities.Business>().UpdateAsync(businessDetailsToUpdate);
                            await _iunitOfWork.Commit(cancellationToken);
                        }
                        else
                        {
                            await _iunitOfWork.Repository<Ensuranx.Domain.Entities.UserBusinessPackage>().AddAsync(userBusinessPackage);
                            await _iunitOfWork.Commit(cancellationToken);

                            var businessEntity = businessMapper.MappingBusinessEntity(addBusinessPackage.BusinessName, userId, addBusinessPackage.BusinessTypeId, email);

                            await _iunitOfWork.Repository<Ensuranx.Domain.Entities.Business>().AddAsync(businessEntity);
                            await _iunitOfWork.Commit(cancellationToken);
                        }

                        dbContextTransaction.Commit();
                        _logger.LogInformation("User Business Package created successfully {@userBusinessPackage}", userBusinessPackage);
                        return new GenericMessage { Message = Constants.APIErrorMessages.PACKAGE_ADDED_SUCESSFULLY };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
    }
}
