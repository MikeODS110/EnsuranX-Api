using Azure;
using Ensuranx.Application.Response.Branch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Infrastructure.Mappers
{
    public class BranchVenueList
    {
        public List<BranchesVenuesResponse> MapBranchVenueResponse(List<GetBranchesVenuesListWithoutTeams> branchVenueListWithoutTeam, List<GetAllTeamsByVenueId> GetAllTeamsByVenueId)
        {
            List<BranchesVenuesResponse> branchesVenuesResponses = new List<BranchesVenuesResponse>();
            branchVenueListWithoutTeam=branchVenueListWithoutTeam.DistinctBy(x => x.venueId).ToList();

            foreach (GetBranchesVenuesListWithoutTeams venue in branchVenueListWithoutTeam)
            {
                
                List<GetAllTeamsByVenueId> filteredTeams = GetAllTeamsByVenueId.DistinctBy(x=>x.Id).ToList();
                

                BranchesVenuesResponse response = new BranchesVenuesResponse();
                //response.ActiveTeams = new List<GetAllTeamsByVenueId>();

                response.Id = venue.venueId;
                response.venueId = venue.venueId; 
                response.VenueName = venue.VenueName;
                response.BranchName = venue.BranchName;
                response.CurrentActivity = venue.CurrentActivity;
                response.Status = venue.Status;
                response.StatskeeperName = venue.StatskeeperName;
               // response.StatskeeperContact= venue.StatskeeperContact;
                response.Status=venue.Status;
                response.CurrentActivity= venue.CurrentActivity;
              //  response.ActiveTeams.AddRange(filteredTeams);
              // response.ActivePlayers = response.ActiveTeams.Sum(t => t.Members); // getting response.ActiveTeams.Sum(t => t.Members) in int
              
                branchesVenuesResponses.Add(response);
            }
            return branchesVenuesResponses;
        }
    }
}
