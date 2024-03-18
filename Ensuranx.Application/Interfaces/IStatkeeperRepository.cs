using Ensuranx.Application.Requests.Statkeeper;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IStatkeeperRepository
    {
        public Task<List<UserInfo>> GetAllStatkeepersList(long venueId);
        public Task<List<VenuesStatskeeper>> GetAllStatkeepersByUserIdList(long userId, BasicFilter paginationFilter);

        public Task<SubstitutePlayer> SubstitutePlayer(long teamId, long eventId, long substituteId, long substituteToId);
        public Task<bool> IsMember(long teamId, long teamMember);
        public Task<Application.Response.User.SubstitutePlayer> Check(long teamId,long eventId, long substituteId, long substituteToId);
        public Task<List<GenericObj>> getVenuesByStatkeeperId(long statkeeperId);
        public Task<UpdateStakeeper> GetStatkeeperBio(long statkeeprId);
        public Task<UserInfo> GetUserTbl(long statkeeperId);
    }
}
