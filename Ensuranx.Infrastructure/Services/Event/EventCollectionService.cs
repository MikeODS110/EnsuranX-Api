using ErrorOr;
using Ensuranx.Application.Contracts.Event;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Event;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Other;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Services.Event
{
    /// <summary>
    /// actions related to Event Collection Entity
    /// </summary>
    public class EventCollectionService : IEventCollectionService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;
        private readonly IVenueActivityRepository _ivenueActivityRepository;
        private readonly IRoleRepository _iroleRepository;
        private readonly IActivityRepository _iactivityRepository;

        public EventCollectionService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _ivenueActivityRepository = new VenueActivityRepository(context);
            _iactivityRepository = new ActivityRepository(context);
            _iroleRepository = new RoleRepository(context);
        }

        /// <summary>
        /// creates lobby for teams to join.Event Collection is the table for lobby
        /// </summary>
        /// <param name="userId">statkeeper id</param>
        /// <param name="email">statkeeper email</param>
        /// <param name="venueId">venue in which lobby is created</param>
        /// <param name="activityId">activity which will be played</param>
        /// <param name="evenTypeId">shows which event will created either pickup or regular</param>
        /// <returns>string success message or error problem</returns>
        public async Task<ErrorOr<string>> CreateEventCollectionAsync(long userId, string email, CreateEventCollection createEventCollection, long roleId)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            EventCollectionMapper eventCollectionMapper = new EventCollectionMapper();
            var Role = await _iroleRepository.GetRole(userId, roleId);

            try
            {
                if (Role == Domain.Enums.Enum.Roles.Statkeeper.ToString())
                {
                    Domain.Entities.Activity activity = await _iactivityRepository.GetActivityByEventTypeAndActivityCategoryId(createEventCollection.EventTypeId, createEventCollection.ActivityCategoryId);

                    EventCollection eventCollection = eventCollectionMapper.MapEventCollectionForCreate(createEventCollection.VenueId, activity.Id, createEventCollection.EventTypeId, email, userId);
                    eventCollection = await _iunitOfWork.Repository<Domain.Entities.EventCollection>().AddAsync(eventCollection);
                    await _iunitOfWork.Commit(cancellationToken);

                    _logger.LogInformation("Event Collection is created successfully {@eventCollection}", eventCollection);
                    return Constants.APIErrorMessages.LOBBY_CREATED;
                }
                else
                {
                    return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            } 
            
        }

        /// <summary>
        /// Lock Event collection/Lobby for further teams to join
        /// </summary>
        /// <param name="userId">user id who is requesting the api</param>
        /// <param name="email">user email who is requesting the api</param>
        /// <param name="eventCollectionId">lobby/event collection id</param>
        /// <returns>problem statement in case of error and string message in case of success</returns>
        public async Task<ErrorOr<LockLobby>> LockEventCollectionAsync(long userId, string email, long eventCollectionId)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {
                EventCollection eventCollection = await _iunitOfWork.Repository<Domain.Entities.EventCollection>().GetByIdAsync(eventCollectionId);

                if (eventCollection is null)
                {
                    return Domain.Common.Errors.Errors.Event.LobbyNotFound;
                }
                else
                {
                    eventCollection.IsLocked = eventCollection.IsLocked != true ? true : false;
                    await _iunitOfWork.Repository<Domain.Entities.EventCollection>().UpdateAsync(eventCollection);
                    await _iunitOfWork.Commit(cancellationToken);

                    _logger.LogInformation("Event collection locked status change successfully");
                    return new LockLobby { IsLock = eventCollection.IsLocked, Message = Constants.APIErrorMessages.LOBBY_STATUS_UPDATED };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            }
        }
    }
}
