using Ensuranx.Application.Response.Device;
using Ensuranx.Domain.Entities;
using Mapster;

namespace Ensuranx.Infrastructure.Mappers
{
    public class DeviceMapper
    {
        public WaitingList AddIntoWaitingList(long userId, long eventCollectionId, string email,int playerNumber)
        {
            WaitingList waitingList = new WaitingList();

            waitingList.UserInfoId = userId;
            waitingList.EventCollectionId = eventCollectionId;
            waitingList.CreatedBy = email;
            waitingList.LastModifiedBy = email;
            waitingList.CreatedDateTime = DateTime.UtcNow;
            waitingList.LastModifiedDateTime = DateTime.UtcNow;
            waitingList.PlayerNumber = playerNumber != 0 ? playerNumber + 1 : 1;
            
            return waitingList;

        }

        /// <summary>
        /// increment string by one
        /// </summary>
        /// <param name="sequenceIncrement"></param>
        /// <returns></returns>
        public string IncrementSequence(string sequenceIncrement)
        {
            int number = Convert.ToInt32(sequenceIncrement);
            number += 1;
            string str = number.ToString("D2");
            return str;
        }

        public List<GetWaitingList> AssignSequenceToWaitingList(List<WaitingList> waitingList)
        {
            List<GetWaitingList> getWaitingList = new List<GetWaitingList>();

            string sequence = "00";

            for (int i = 0; i < waitingList.Count; i++)
            {
                GetWaitingList getWaiting = new GetWaitingList();

                getWaiting.Id = waitingList[i].UserInfoId;
                getWaiting.Name = waitingList[i].UserInfo.Name;
                getWaiting.Sequence = IncrementSequence(sequence);

                sequence = getWaiting.Sequence;
                if (waitingList[i].IsDeleted != true && waitingList[i].IsActive != false)
                {
                    getWaitingList.Add(getWaiting);
                }
                
            }

            return getWaitingList;
        }

        public GetDeviceInfo MapDeviceInfo(EventCollection eventCollection)
        {
            GetDeviceInfo getDeviceInfo = new GetDeviceInfo();

            getDeviceInfo.ActivityId = eventCollection.Activity.ActivityCategoryId;
            getDeviceInfo.ActivityName = eventCollection.Activity.ActivityCategory.Name;
            getDeviceInfo.EventTypeId = eventCollection.EventTypeId;
            getDeviceInfo.EventTypeName = eventCollection.EventType.Name;
            getDeviceInfo.VenueId = eventCollection.VenueId;
            getDeviceInfo.VenueName = eventCollection.Venue.Name;

            return getDeviceInfo;
        }
        
        public List<WaitingList> CreateNewWaitingListPlayerList(List<long> playerListToAddToWaitingList,long eventCollectionId,string email)
        {
            List<WaitingList> waitingList = new List<WaitingList>();

            foreach (long playerId in playerListToAddToWaitingList)
            {
                WaitingList waitPlayer = new WaitingList();

                waitPlayer.EventCollectionId = eventCollectionId;
                waitPlayer.UserInfoId = playerId;
                waitPlayer.CreatedBy = email;
                waitPlayer.LastModifiedBy = email;
                waitPlayer.LastModifiedDateTime = DateTime.UtcNow;
                waitPlayer.CreatedDateTime = DateTime.UtcNow;

                waitingList.Add(waitPlayer);
            }

            return waitingList;
        }
    
        public List<WaitingListTeam> MapTournamentWaitingListResponse(List<GetWaitingList> waitingList, List<TeamMember> teamMemberList)
        {
            List<WaitingListTeam> waitingListTeamList = new List<WaitingListTeam>();

            var teamCount = teamMemberList.Select(x => x.TeamId).Distinct().ToList();

            for(var i = 0; i < teamCount.Count;i++)
            {
                var teamMember = teamMemberList.Where(x => x.TeamId == teamCount[i]).ToList();

                if (teamMember.Any())
                {
                    WaitingListTeam waitingListTeam = new WaitingListTeam();

                    waitingListTeam.TeamId = teamCount[i];
                    waitingListTeam.TeamName = teamMember.First().Team.Name;

                    waitingListTeam.getWaitingList = new List<GetWaitingList>();

                    foreach (var member in teamMember)
                    {
                        var waitListPlayer = waitingList.Where(x => x.Id == member.PlayerId).FirstOrDefault();

                        if (waitListPlayer is not null)
                        {
                            GetWaitingList getWaitingList = new GetWaitingList();
                            getWaitingList = waitListPlayer.Adapt<GetWaitingList>();
                            getWaitingList.Sequence = member.UserInfo.JerseyNo != "0" ? member.UserInfo.JerseyNo : getWaitingList.Sequence;
                            waitingListTeam.getWaitingList.Add(getWaitingList);
                        }
                    }

                    waitingListTeamList.Add(waitingListTeam);
                }
            }

            return waitingListTeamList;
        }
    
    }
}
