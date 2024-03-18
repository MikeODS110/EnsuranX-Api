using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Venue;
using Ensuranx.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Infrastructure.Mappers
{
    public class VenueEventList
    {
        public List<VenueEventListResponse> MapVenueEventResponse(List<GetVenuesEventWithoutTeams> branchVenueListWithoutTeam, List<GetAllTeamsByVenueId> GetAllTeamsByVenueId)
        {
            List<VenueEventListResponse> VenueEventListResponse = new List<VenueEventListResponse>();
            branchVenueListWithoutTeam = branchVenueListWithoutTeam.DistinctBy(x=>x.venueId).ToList();

            foreach (GetVenuesEventWithoutTeams venue in branchVenueListWithoutTeam)
            {

                List<GetAllTeamsByVenueId> filteredTeams = GetAllTeamsByVenueId.DistinctBy(x => x.Id).ToList();


                VenueEventListResponse response = new VenueEventListResponse();
                

                response.Id = venue.Id;
                response.venueId = venue.venueId;
                response.VenueName = venue.VenueName;
                response.BranchName = venue.BranchName;
                response.CurrentActivity = venue.CurrentActivity;
                response.Status = venue.Status;
                response.StatskeeperName = venue.StatskeeperName;
                response.StatskeeperContact = venue.StatskeeperContact;
                response.Status = venue.Status;
                response.CurrentActivity = venue.CurrentActivity;
               

                VenueEventListResponse.Add(response);
            }
            return VenueEventListResponse;
        }
    }
}
