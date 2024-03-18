using Ensuranx.Application.Response.Team;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface ITournamentTeamRepository
    {
        public IQueryable<TournamentTeam> GetAll();
        public Task<TournamentTeam> CheckTournamentTeamRequestAsync(long tournamentId, long teamId);
        public Task<TournamentTeam> GetUserTeamJoinedInTheTournamentAsync(long tournamentId, long userId);
        public int CheckTournamentTeamRequestAcceptedCountAsync(long tournamentId);
        public Task<List<TournamentTeam>> GetTournamentTeamAllRequestAcceptedAsync(long tournamentId);

        public Task<List<RepeatedJersey>> GetRepeatedJersey(long teamId);

        public long GetTournamentBusinesId(long tournamentId);
    }
}
