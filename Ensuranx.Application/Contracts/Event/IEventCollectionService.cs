using ErrorOr;
using Ensuranx.Application.Requests.Event;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Other;

namespace Ensuranx.Application.Contracts.Event
{
    public interface IEventCollectionService
    {
        public Task<ErrorOr<string>> CreateEventCollectionAsync(long userId, string email, CreateEventCollection createEventCollection, long roleId);
        public Task<ErrorOr<LockLobby>> LockEventCollectionAsync(long userId, string email, long eventCollectionId);
    }
}
