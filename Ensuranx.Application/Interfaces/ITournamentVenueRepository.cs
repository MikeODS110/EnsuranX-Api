using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface ITournamentVenueRepository
    {
        public Task<List<TournamentVenue>> GetAllTournamentVenueAsync(long tournamentId);
        public Task<List<TournamentVenue>> CreateTournamentVenueListAsync(List<TournamentVenue> tournamentVenueList);
        public Task<List<TournamentStatkeeper>> CreateTournamentStatkeeperListAsync(List<TournamentStatkeeper> tournamentVenueList);
        public IQueryable<TournamentVenue> GetAll();
    }
}
