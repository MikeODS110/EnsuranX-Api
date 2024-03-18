using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Ensuranx.Domain.IdentityExtensions;
using System.Linq;
using System.Reflection.Metadata;

namespace Ensuranx.Infrastructure.Mappers
{
    public class VenueStatkeeperMapper
    {
        public List<VenuesStatskeeper> MapVenueWithStatkeeperList(string email, long venueId, List<long> StatkeeperIdList, DateTime createdDatetime)
        {
            List<VenuesStatskeeper> venuesStatskeeperList = new List<VenuesStatskeeper>();

            foreach (var Statkeeper in StatkeeperIdList)
            {
                VenuesStatskeeper venuesStatskeeper = new VenuesStatskeeper();

                venuesStatskeeper.UserInfoId = Statkeeper;
                venuesStatskeeper.VenueId = venueId;
                venuesStatskeeper.CreatedBy = email;
                venuesStatskeeper.LastModifiedBy = email;
                venuesStatskeeper.CreatedDateTime = createdDatetime;
                venuesStatskeeper.LastModifiedDateTime = createdDatetime;

                venuesStatskeeperList.Add(venuesStatskeeper);
            }


            return venuesStatskeeperList;
        }


        public List<VenuesStatskeeper> MapStatkeeperWithVenueList(string email, List<long> venueList, long statkeeperId, DateTime createdDatetime)
        {
            List<VenuesStatskeeper> venuesStatskeeperList = new List<VenuesStatskeeper>();

            foreach (var venue in venueList)
            {
                VenuesStatskeeper venuesStatskeeper = new VenuesStatskeeper();

                venuesStatskeeper.UserInfoId = statkeeperId;
                venuesStatskeeper.VenueId = venue;
                venuesStatskeeper.CreatedBy = email;
                venuesStatskeeper.LastModifiedBy = email;
                venuesStatskeeper.CreatedDateTime = createdDatetime;
                venuesStatskeeper.LastModifiedDateTime = createdDatetime;

                venuesStatskeeperList.Add(venuesStatskeeper);
            }
            return venuesStatskeeperList;
        }

        public List<AllStatkeepersDetails> MapAllStatkeepersDetailsResponse(List<UserInfo> userInfoList, List<Venue> venueList,
            List<VenuesStatskeeper> venuesStatskeeperList, List<Images> imageList, List<AppUser> appUserList, List<Branch> branchList,
            List<ImageType> imageTypeList)
            
        {
            List<AllStatkeepersDetails> allStatkeepersDetailList = new List<AllStatkeepersDetails>();


            foreach (UserInfo userInfo in userInfoList)
            {
                AllStatkeepersDetails allStatkeepersDetail = new AllStatkeepersDetails();

                var venues = venuesStatskeeperList.Where(x => x.UserInfoId == userInfo.Id).Select(x => x.VenueId).ToList();
                var user = appUserList.Where(x => x.Id == userInfo.UserId).FirstOrDefault();

                var profileImageType = imageTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();
                var coverImageType = imageTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ImageType.Cover.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                var userProfileImage = imageList.Where(x => x.TableId == userInfo.Id && x.ImageTypeId == profileImageType)
                                                .Select(x => x.Path).FirstOrDefault();

                var userCoverImage = imageList.Where(x => x.TableId == userInfo.Id && x.ImageTypeId == coverImageType)
                                                .Select(x => x.Path).FirstOrDefault();

                if (venues.Any())
                {
                    var vanuel = venueList.Where(x => venues.Contains(x.Id)).FirstOrDefault();
                    allStatkeepersDetail.VenueName = vanuel.Name;
                    allStatkeepersDetail.BranchName = branchList.Where(x => x.Id == vanuel.BranchId).Select(x => x.Name).FirstOrDefault();
                }

                allStatkeepersDetail.ProfileImage = !string.IsNullOrEmpty(userProfileImage) ? userProfileImage
                                                    : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;

                allStatkeepersDetail.CoverImage = !string.IsNullOrEmpty(userCoverImage) ? userCoverImage
                                                  : Constants.APIErrorMessages.USER_DEFAULT_COVER_IMAGE;

                allStatkeepersDetail.UserId = userInfo.Id;
                allStatkeepersDetail.Name = userInfo.Name;
                allStatkeepersDetail.Email = user.Email;
                allStatkeepersDetail.PhoneNumber = user.PhoneNumber;

                allStatkeepersDetailList.Add(allStatkeepersDetail);
            }

            return allStatkeepersDetailList;
        }

        public GetStatkeeperById MapGetStatkeeperById(List<GetVenueBrnach> venueList, List<GenericObj> branchStatkeeper, UserInfo userInfo)
        {
            GetStatkeeperById getStatkeeperById = new GetStatkeeperById();
           

            getStatkeeperById.Name = userInfo.Name;
            getStatkeeperById.Email = userInfo.User.Email;
            getStatkeeperById.PhoneNumber = userInfo.User.PhoneNumber;
            getStatkeeperById.VenueIdList = new List<GetVenueBrnach>();
            getStatkeeperById.BranchId = new List<GenericObj>();

            foreach (GetVenueBrnach venue in venueList)
            {              
                getStatkeeperById.VenueIdList.Add(venue);
            }
            foreach (GenericObj branch in branchStatkeeper)
            {
                getStatkeeperById.BranchId.Add(branch);
            }

            return getStatkeeperById;
        }
    }
}
