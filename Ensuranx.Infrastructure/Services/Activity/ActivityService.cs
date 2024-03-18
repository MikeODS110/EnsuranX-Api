using ErrorOr;
using Ensuranx.Application.Contracts.Activity;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Activity;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Ensuranx.Infrastructure.Services.Activity
{
    /// <summary>
    /// All the actions related to activity entity
    /// </summary>
    public class ActivityService : IActivityService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;
        private readonly IActivityRepository _iactivityRepository; 
        private readonly IRoleRepository _iroleRepository;
        private readonly IImagesRepository _iimagesRepository;


        public ActivityService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _iactivityRepository = new ActivityRepository(context);
            _iroleRepository = new RoleRepository(context);
            _iimagesRepository = new ImagesRepository(context);
        }

        /// <summary>
        /// gets all the activities in the system or activity entity
        /// </summary>
        /// <param name="paginationFilter">holds pagesize and pagenumber for pagination</param>
        /// <param name="userId">user id of user which is requesting activities</param>
        /// <param name="venueId">if not 0 gets all the activities else returns activities within the particular venue</param>
        /// <returns></returns>
        public async Task<ErrorOr<List<ActivityResponse>>> GetAllActivitiesAsync(BasicFilter paginationFilter, long userId, long roleId,long venueId)
        {
            
            try{
                var Role = await _iroleRepository.GetRole(userId, roleId);
                if (Role == Domain.Enums.Enum.Roles.Statkeeper.ToString())
                {
                    var result = await _iactivityRepository.GetAllActivityByStatkeeper(paginationFilter, userId,venueId);
                    List<ActivityResponse> responses = result.Adapt<List<ActivityResponse>>();
                
                    return responses;
                }
                else if (Role == Domain.Enums.Enum.Roles.Business.ToString() || Role == Domain.Enums.Enum.Roles.Player.ToString())
                {
                    var result = await _iactivityRepository.GetAllActivityAsyncByBusiness(paginationFilter, userId,venueId);

                    return result;
                }
                else return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            }
        }

    }
}
