using ErrorOr;
using Ensuranx.Application.Requests.Event;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Other;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using System.Threading.Tasks;

namespace Ensuranx.Application.Contracts.Event
{
    public interface IEventService
    {
        public Task<ErrorOr<EventResponse>> CreateEventAsync(long userId, string email, CreateEvent createEvent);
        public Task<ErrorOr<GenericMessage>> StartEventAsync(long userId, string email, long eventId);
        public Task<ErrorOr<EventScoreboad>> GetEventScoreboardAsync(long eventId, long userId, string email);
        public Task<ErrorOr<EventResultResponse>> GetEventResultAsync(long eventId, long userId, string email);
        public Task<ErrorOr<UpdateEventScoreboardResponse>> UpdateEventScoreboardAsync(UpdateEventScoreboard updateEventScoreboard, long userId, string email);
        public Task<ErrorOr<EndEventResponse>> EndEventAsync(long eventId, long userId, string email);

        public Task<ErrorOr<GenericMessage>> GetEndEventCollection(long eventCollectionId, long userId, string email);
        public Task<ErrorOr<LockLobby>> EventCollectionLockStatusAsync(long eventCollectionId, long userId, string email);

        public Task<ErrorOr<MatchSummaryResp>> GetMatchSummary(BasicFilter paginationFilter, long eventId, long userId, long quaterId);
    }
}
