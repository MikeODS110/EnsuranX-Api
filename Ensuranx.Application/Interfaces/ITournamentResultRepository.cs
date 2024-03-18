using Ensuranx.Application.Response.Tournament;

namespace Ensuranx.Application.Interfaces
{
    public interface ITournamentResultRepository
    {
        public Task<List<GetTournamentResult>> GetTournamentResultByTournamentIdAsync(long tournamentId,long userId);
    }
}
