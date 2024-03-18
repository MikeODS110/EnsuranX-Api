using Ensuranx.Application.Requests.Profile;
using Ensuranx.Application.Response.User;
using Ensuranx.Domain.Entities;
using Mapster;

namespace Ensuranx.Infrastructure.Mappers
{
    public class UserInterestMapping
    {
        public List<PlayerInterestActivity> MappingPlayerInterestActivityList(UserInterestsActivities userInterestsActivities,long userId,string email)
        {
            List < PlayerInterestActivity > playerInterestActivities = new List < PlayerInterestActivity > ();
            foreach (var activityCategory in userInterestsActivities.ActivityCategoryIdList)
            {
                PlayerInterestActivity playerInterestActivity = new PlayerInterestActivity();
                playerInterestActivity.UserInfoId = userId;
                playerInterestActivity.ActivityCategoryId = activityCategory;
                playerInterestActivity.CreatedBy = email;
                playerInterestActivity.LastModifiedBy = email;
                playerInterestActivity.CreatedDateTime = DateTime.UtcNow;
                playerInterestActivity.LastModifiedDateTime = DateTime.UtcNow;

                playerInterestActivities.Add(playerInterestActivity);
            }
            return playerInterestActivities;
        }

        public PlayerInterestsResponse MapPlayerInterestActivity(List<PlayerInterestActivity> playerInterestActivityList, Domain.Entities.UserInfo userInfo)
        {
            PlayerInterestsResponse playerInterestsResponse = new PlayerInterestsResponse();

            if (playerInterestActivityList.Any())
            {
                playerInterestsResponse.ActivityCategoryIdList = new List<ActivityListModel>();
                for (int i = 0; i < playerInterestActivityList.Count; i++)
                {
                    ActivityListModel activityListModel = new ActivityListModel();

                    activityListModel.ActivityId = playerInterestActivityList[i].ActivityCategoryId;
                    activityListModel.ActivityName = playerInterestActivityList[i].ActivityCategory.Name;

                    playerInterestsResponse.ActivityCategoryIdList.Add(activityListModel);  
                }
            }
            playerInterestsResponse.ZipCode = userInfo.ZipCode;

            return playerInterestsResponse;
        }
    }
}
