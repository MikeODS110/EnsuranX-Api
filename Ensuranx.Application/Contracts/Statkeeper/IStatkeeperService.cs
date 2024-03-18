using ErrorOr;
using Ensuranx.Application.Requests.Statkeeper;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;

namespace Ensuranx.Application.Contracts.Statkeeper
{
    public interface IStatkeeperService
    {
        public Task<ErrorOr<List<AllStatkeeperResponse>>> GetAllStatkeeperAsync(long venueId,long userId);
        public Task<ErrorOr<GetStatkeeperById>> GetStatkeeperByIdAsync(long userId);
        public Task<ErrorOr<List<AllStatkeepersDetails>>> GetAllStatkeeperByUserIdAsync(long userId,BasicFilter paginationFilter);
        public Task<ErrorOr<AddStatkeeperResponse>> AddStatkeeperAsync(long userId,string email, AddStatkeeper addStatkeeper);

        public Task<ErrorOr<DeleteStatkeeperById>> DeleteStatkeeperByIdAsync(long userId);

        public Task<ErrorOr<DeleteStatkeeperById>> DeleteStatkeeperByVenueId(long venueId, long statkeeperId);
        public Task<ErrorOr<GenericMessage>> SubstitutePlayer(long teamId, long eventId, long substituteId,long substituteToId,string email,bool isDisqaulify);
        public Task<ErrorOr<List<GenericObj>>> GetVenueByStatkeeperIdAsync(long userId);
        public Task<ErrorOr<GenericMessage>> UpdateStatkeeperAsyn(long userId,string email,UpdateStakeeper updateStakeeper);
    }

}
