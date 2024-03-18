using ErrorOr;
using Ensuranx.Application.Contracts.UserInterestActivity;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Profile;
using Ensuranx.Application.Response.User;
using Ensuranx.Domain.Entities;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Ensuranx.Infrastructure.Services.UserInterestActivity
{
    public class UserInterestActivityService : IUserInterestActivityService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly IPlayerInterestActivityRepository _iplayerInterestActivityRepository;
        private readonly IActivityRepository _iactivityRepository;
        private readonly ILogger _logger;

        public UserInterestActivityService(ApplicationDbContext context, ILogger logger, UserManager<AppUser> userManager)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _iplayerInterestActivityRepository = new PlayerInterestActivityRepository(context);
            _iactivityRepository = new ActivityRepository(context);
        }

        public async Task<ErrorOr<PlayerInterestActivity>> AddNewPlayerInterestActivity(PlayerInterestActivity playerInterestActivity)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            playerInterestActivity = await _iunitOfWork.Repository<PlayerInterestActivity>().AddAsync(playerInterestActivity);
            await _iunitOfWork.Commit(cancellationToken);
            return playerInterestActivity;
        } 
        
        public async Task<ErrorOr<List<PlayerInterestActivity>>> AddNewPlayerInterestActivityList(UserInterestsActivities userInterestsActivities,long userId,string email)
        {
            UserInterestMapping userInterestMapping = new UserInterestMapping();
            List<PlayerInterestActivity> playerInterestActivitys = new List<PlayerInterestActivity>();
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {

                playerInterestActivitys = userInterestMapping.MappingPlayerInterestActivityList(userInterestsActivities, userId, email);
                _logger.LogInformation("New Player Interest Activity Mapped {@playerInterestActivity}", playerInterestActivitys);

                try
                {
                    bool res = await _iplayerInterestActivityRepository.DeleteUserInterest(userId);
                    _logger.LogInformation("users all interest deleted now creating new interests");

                    foreach (var player in playerInterestActivitys)
                    {
                        await AddNewPlayerInterestActivity(player);
                    }

                    Domain.Entities.UserInfo userInfo = await _iunitOfWork.Repository<Domain.Entities.UserInfo>().GetByIdAsync(userId);

                    if (userInfo is not null && userInfo.ZipCode != userInterestsActivities.ZipCode)
                    {
                        userInfo.ZipCode = userInterestsActivities.ZipCode;
                        await _iunitOfWork.Repository<Domain.Entities.UserInfo>().UpdateAsync(userInfo);
                        await _iunitOfWork.Commit(cancellationToken);

                        _logger.LogInformation("userInfo update successfully {@userInfo}", userInfo);
                    }
                }
                catch (Exception ex)
                {
                    return Domain.Common.Errors.Errors.User.DuplicateInterest;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }

            _logger.LogInformation("Player interests successfully added {@playerInterestActivitys}", playerInterestActivitys);
            return playerInterestActivitys;
        }

        public async Task<ErrorOr<PlayerInterestsResponse?>> GetUserInterestsById(long userId)
        {
            UserInterestMapping userInterestMapping = new UserInterestMapping();
            PlayerInterestsResponse playerInterestsResponse = new PlayerInterestsResponse();

            try
            {
                _logger.LogInformation($"Going to fetch players interests with userid {userId}");
                List<PlayerInterestActivity> playerInterestActivityList = await _iplayerInterestActivityRepository.GetPlayerInterestsAcititiesById(userId);
                Domain.Entities.UserInfo userInfo = await _iunitOfWork.Repository<Domain.Entities.UserInfo>().GetByIdAsync(userId);
                playerInterestsResponse = userInterestMapping.MapPlayerInterestActivity(playerInterestActivityList, userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }

            _logger.LogInformation("player interests response {@playerInterestsResponse}", playerInterestsResponse);
            return playerInterestsResponse ;
        }
    }
}
