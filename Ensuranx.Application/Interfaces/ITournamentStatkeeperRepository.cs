using Ensuranx.Application.Response.User;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface ITournamentStatkeeperRepository
    {
        public Task<List<AllStatkeepersVenueList>> GetStatkeeprList(long tournamentId);
        public Task<TournamentStatkeeper> GetTournamentStatkeeperByIdsAsync(long tournamentId, int statkeeperId);
    }
}
