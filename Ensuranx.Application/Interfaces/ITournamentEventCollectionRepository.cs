using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces;

public interface ITournamentEventCollectionRepository
{
    public Task<List<TournamentEventCollection>> CreateTournamentEventCollectionList(List<TournamentEventCollection> tournamentEventCollectionList);
    public Task<List<TournamentEventCollection>> GetTournamentEventCollectionByTournamentIdList(long tournamentId);
    public Task<TournamentEventCollection> GetTournamentEventCollectionByEventCollectionId(long eventCollectionId);
    public Task<long> GetTournamentEventCollectionIdByEventCollectionId(long eventCollectionId);
}
