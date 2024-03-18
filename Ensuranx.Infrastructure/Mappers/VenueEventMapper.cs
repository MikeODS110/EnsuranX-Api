using Ensuranx.Application.Requests.Venue;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Venue;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class VenueEventMapper
    {
        public List<VenueEventListResponse> MapVenueEventResponse(List<GetVenuesEventWithoutTeams> branchVenueListWithoutTeam, List<GetAllPlayersByEC> teamsPlayer)
        {
            List<VenueEventListResponse> VenueEventListResponse = new List<VenueEventListResponse>();
            
            branchVenueListWithoutTeam = branchVenueListWithoutTeam.DistinctBy(x => x.Id).OrderByDescending(x => x.Id).ToList();

            List<GetAllPlayersByEC> getAllPlayers = new List<GetAllPlayersByEC>();
            getAllPlayers = teamsPlayer.ToList();

            foreach (GetVenuesEventWithoutTeams venue in branchVenueListWithoutTeam)
            {
                if(venue.Status == Domain.Enums.Enum.EventStatus.Win.ToString()  || 
                   venue.Status == Domain.Enums.Enum.EventStatus.Lose.ToString() ||
                   venue.Status == Domain.Enums.Enum.EventStatus.Tie.ToString() )
                {
                    venue.Status = Domain.Enums.Enum.EventStatus.Completed.ToString();
                }  
                List<GetAllPlayersByEC> filterPlayers = teamsPlayer.Where(x=>x.Id== venue.Id).ToList(); 
                VenueEventListResponse response = new VenueEventListResponse();
                
                response.AllPlayers = new List<GetAllPlayersByEC>();

                response.Id = venue.Id;
                response.venueId = venue.venueId;
                response.VenueName = venue.VenueName;
                response.BranchName = venue.BranchName;
                response.CurrentActivity = venue.CurrentActivity;
                response.ActivityIcon = venue.ActivityIcon;
                response.Status = venue.Status;
                response.StatskeeperName = venue.StatskeeperName;
                response.StatskeeperImage = venue.StatskeeperImage;
                response.StatskeeperContact = venue.StatskeeperContact;
                response.Status = venue.Status;
                response.CreatedDate = venue.CreatedDate;
                response.AllPlayers.AddRange(filterPlayers);
                response.ActivePlayers = filterPlayers.Select(x => x.PlayerCount).FirstOrDefault();
                VenueEventListResponse.Add(response);
            }
            return VenueEventListResponse;
        }

        public EventCollection MapEventCollection(EventCollection events, UpdateEventVenueListReq VenueEventListUpdateReq)
        {
            events.StatKeeperId = VenueEventListUpdateReq.StatsKeeperId;
            events.ActivityId = VenueEventListUpdateReq.activityId;
            events.LastModifiedDateTime = DateTime.UtcNow;

            return events;
        }

        public EventCollection MapBranchVenue(EventCollection EventCollection, UpdateVenueStatkeeperReq UpdateVenueStatkeeperReq)
        {
            EventCollection.VenueId= UpdateVenueStatkeeperReq.venueId;
            EventCollection.ActivityId = UpdateVenueStatkeeperReq.activityId;
            EventCollection.StatKeeperId = UpdateVenueStatkeeperReq.StatsKeeperId;
            EventCollection.LastModifiedDateTime = DateTime.UtcNow;

            return EventCollection;
        }

        public List<VenueByBrandIdResp> GetAllVenuesRespMapper(List<VenueByBrandIdResp> getVenuesDetails, 
               List<currentActivityResp> currentActivityResps,List<StatkeeperImagesDetails> statkeeperImagesDetails,
               List<GetAllPlayersByEC> getAllPlayersByECs)
        {
            List<VenueByBrandIdResp> VenueResponse = new List<VenueByBrandIdResp>();
            var StatkeeperBranchId = statkeeperImagesDetails.Select(x => x.BranchId).ToList();
        
            foreach (VenueByBrandIdResp response in getVenuesDetails)
            {

                VenueByBrandIdResp allVenueResponse = new VenueByBrandIdResp();
                StatkeeperImagesDetails statkeeperImagess = new StatkeeperImagesDetails();

                allVenueResponse.Id = response.Id;
                allVenueResponse.Name = response.Name;
                allVenueResponse.CreatedDate = response.CreatedDate;
                currentActivityResp currentActivityResp = currentActivityResps.FirstOrDefault(x => x.venueId == response.Id);
                if (currentActivityResp != null)
                {
                    allVenueResponse.CurrentActivity = currentActivityResp.currentActName;
                    allVenueResponse.CurrentActivityImageUrl = currentActivityResp.CurrentActivityImageUrl;
                    GetAllPlayersByEC GetAllPlayersByEC = getAllPlayersByECs.FirstOrDefault(x=>x.Id == currentActivityResp.eventId);
                    if(GetAllPlayersByEC != null)
                    {
                        allVenueResponse.PlayerCount = GetAllPlayersByEC.PlayerCount;
                    }

                }
                List<StatkeeperImagesDetails> matchingStatkeeperImagesDetails = statkeeperImagesDetails
                               .Where(x => x.BranchId == response.Id)
                               .ToList();

                allVenueResponse.StatkeeperDetails.AddRange(matchingStatkeeperImagesDetails);

                

                VenueResponse.Add(allVenueResponse);
             
            }
            return VenueResponse;
        }
    }
}
