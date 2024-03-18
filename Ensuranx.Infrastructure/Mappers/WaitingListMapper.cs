using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers;

public class WaitingListMapper
{
    public WaitingList CreateWaitingListMap(string email,long userId,long eventCollectionId,int lastWaitingListPlayer)
    {
        WaitingList waitingList = new WaitingList();

        waitingList.EventCollectionId = eventCollectionId;
        waitingList.UserInfoId = userId;
        waitingList.CreatedBy = email;
        waitingList.LastModifiedBy = email;
        waitingList.CreatedDateTime = DateTime.UtcNow;
        waitingList.LastModifiedDateTime = DateTime.UtcNow;
        waitingList.PlayerNumber = lastWaitingListPlayer != 0 ? lastWaitingListPlayer + 1 : 1;

        return waitingList;
    }
}
