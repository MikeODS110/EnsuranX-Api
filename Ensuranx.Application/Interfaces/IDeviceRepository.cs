using Ensuranx.Application.Response.Device;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IDeviceRepository
    {
        public Task<long> GetEventCollection(string deviceToken, string email, long userId);
        public Task<Device> GetDeviceByDeviceId(string deviceToken, long userId);
        public Task<List<WaitingList>> GetWaitingListDeletePlayerList(long eventCollection);
        public Task<List<GetWaitingList>> GetWaitingList(long eventCollection);
        public Task<List<GetWaitingList>> GetWaitingListByEventId(long eventId, long eventCollection);
        public Task<WaitingList> GetWaitingListByPlayerIdDeletedOne(long playerId,long eventCollectionId);
        public Task<WaitingList> GetWaitingListByPlayerId(long playerId);
        public Task<WaitingList> GetWaitingListByEventCollectionIdAndUserId(long eventCollection, long userId);
        public Task<bool> CheckIfThisUserIsAPartOfAnyTeamInTheEventCollection(long eventCollection, long userId);
        public Task<bool> CheckIfThisUserIsAPartOfAnyTeamInTheEventCollectionIsJoinedCollectionFalse(long eventCollection, long userId);
        public Task<int> GetLastPlayerNumberInTheList(long eventCollection);

        public Task<bool> GetWaitingListByLobby(string deviceToken,long userInfoId);
        public Task<List<WaitingList>> ShiftThesePlayerToWaitingListAsync(long eventCollectionId,List<long> userInfoIdList);
        public Task<List<WaitingList>> UpdateWaitingListAsync(List<WaitingList> waitingList);
        public Task<List<WaitingList>> CreateWaitingListAsync(List<WaitingList> waitingList);
        public Task<WaitingList> GetWaitingListByPlayerId(long playerId, long eventCollectionId);
        public Task<WaitingList> GetPlayerFromWaitingList(string deviceToken, long userInfoId);
    }
}
