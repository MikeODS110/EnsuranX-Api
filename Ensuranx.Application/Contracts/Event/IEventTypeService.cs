using ErrorOr;
using Ensuranx.Application.Response.Event;

namespace Ensuranx.Application.Contracts.Event
{
    public interface IEventTypeService
    {
        public Task<ErrorOr<List<EventTypeResponse>>> GetEventTypeAsync();
    }
}
