using ErrorOr;
using Ensuranx.Application.Contracts.Event;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Response.Event;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Ensuranx.Infrastructure.Services.Event
{
    /// <summary>
    /// actions related to Event type entity
    /// </summary>
    public class EventTypeService : IEventTypeService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;
        private readonly IEventRepository _ieventRepository;

        public EventTypeService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _ieventRepository = new EventRepository(context);
        }

        /// <summary>
        /// list of different types of event system contains
        /// </summary>
        /// <returns>Id and Name of event type</returns>
        public async Task<ErrorOr<List<EventTypeResponse>>> GetEventTypeAsync()
        {
            try
            {
                List<EventType> eventTypeList = await _ieventRepository.GetEventTypeAsync();
                List<EventTypeResponse> eventTypeResponse = eventTypeList.Adapt<List<EventTypeResponse>>();
                _logger.LogInformation("eventTypeResponse {@eventTypeResponse}", eventTypeResponse);
                return eventTypeResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            }
        }
    }
}
