using Ensuranx.Application.Requests.Branch;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.BusinessPackage;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Mapster;

namespace Ensuranx.Infrastructure.Mappers
{
    public class BranchesDetailsMapper
    {
        public BusinessDetailsResponse MappingBusinessDetailsResponse(Branch branch,List<BranchScheduling> branchSchedulings,List<Images> imageList,List<Connection> connectionList,List<ImageType> imageTypeList,
            long totalVenues, long totalSports, long totalMatches


            )
        {
            BusinessDetailsResponse businessDetailsResponse = new BusinessDetailsResponse();

            var profileImage = imageList.Where(x => x.ImageType == imageTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower())).FirstOrDefault();
            var coverImage = imageList.Where(x => x.ImageType == imageTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ImageType.Cover.ToString().ToLower())).FirstOrDefault();



            businessDetailsResponse.totalVenues = totalVenues;
            businessDetailsResponse.totalEvents= totalMatches;
            businessDetailsResponse.totalSports= totalSports;
            businessDetailsResponse.BranchDetailsModel1 = new BranchDetailsModel();
            businessDetailsResponse.BranchDetailsModel1.Name = branch.Name;
            businessDetailsResponse.BranchDetailsModel1.Id = branch.Id;

            businessDetailsResponse.BranchDetailsModel1.Profile = profileImage is not null ? profileImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
            businessDetailsResponse.BranchDetailsModel1.Cover = coverImage is not null ? coverImage.Path : Constants.APIErrorMessages.USER_DEFAULT_COVER_IMAGE;
            businessDetailsResponse.BranchDetailsModel1.ProfileId = profileImage is not null ? profileImage.Id : 0;
            businessDetailsResponse.BranchDetailsModel1.CoverId = coverImage is not null ? coverImage.Id : 0;

            foreach (Connection connection in connectionList)
            {
                switch (connection.ConnectionTypeId)
                {
                    case (long)Domain.Enums.Enum.ConnectionType.Twitter:
                        businessDetailsResponse.BranchDetailsModel1.TwitterUrl = connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.Email:
                        businessDetailsResponse.BranchDetailsModel1.Email = connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.Website:
                        businessDetailsResponse.BranchDetailsModel1.WebsiteUrl = connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.Facebook:
                        businessDetailsResponse.BranchDetailsModel1.FacebookUrl = connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.PhoneNumber:
                        businessDetailsResponse.BranchDetailsModel1.PhoneNumber = connection.Value;
                        break;
                    case (long)Domain.Enums.Enum.ConnectionType.Address:
                        businessDetailsResponse.BranchDetailsModel1.Address = connection.Value;
                        break;
                    default:
                        break;
                }
            }


            businessDetailsResponse.BranchSchedulingDetails = new List<BranchSchedulingDetails>();

            foreach (var branchScheduling in branchSchedulings)
            {
                BranchSchedulingDetails branchSchedulingDetails = new BranchSchedulingDetails();
                branchSchedulingDetails.Id = branchScheduling.Id;
                branchSchedulingDetails.StartTime = branchScheduling.StartTime.ToString();
                branchSchedulingDetails.EndTime = branchScheduling.EndTime.ToString();
                branchSchedulingDetails.WeekDays = branchScheduling.WeekDay.ToString();
                businessDetailsResponse.BranchSchedulingDetails.Add(branchSchedulingDetails);
            }

            return businessDetailsResponse;
        }


        public List<AllBranchesResponse> GetAllBranchesRespMapper(List<AllBranchesResponse> branch, List<StatkeeperImagesDetails> statkeeperImages)
        {
            List<AllBranchesResponse> branchesResponse = new List<AllBranchesResponse>();
            var StatkeeperBranchId = statkeeperImages.Select(x => x.BranchId).ToList();
       
            foreach (AllBranchesResponse response in branch)
            {
                AllBranchesResponse allBranchesResponse = new AllBranchesResponse();
                StatkeeperImagesDetails statkeeperImagess = new StatkeeperImagesDetails();

                allBranchesResponse.Id = response.Id;
                allBranchesResponse.Name = response.Name;
                allBranchesResponse.ProfileImageUrl = response.ProfileImageUrl;
                allBranchesResponse.CoverImageUrl = response.CoverImageUrl;
                //if (StatkeeperBranchId.Contains(response.Id))
                if (StatkeeperBranchId.Contains(response.Id))
                {

                    // List<StatkeeperImagesDetails> matchingImages = statkeeperImages.Select(x => new StatkeeperImagesDetails { Id = x.Id, Name = x.Name }).ToList();
                    //  allBranchesResponse.StatkeeperDetails.AddRange(statkeeperImagess);
                    var matchingImages = statkeeperImages.Where(x => x.BranchId == response.Id).ToList();
                    allBranchesResponse.StatkeeperDetails.AddRange(matchingImages);
                    //allBranchesResponse.StatkeeperDetails.AddRange(statkeeperImages);
                }
              
                branchesResponse.Add(allBranchesResponse);
            }
            return branchesResponse;
        }
    }
}
