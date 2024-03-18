namespace Ensuranx.Application.Interfaces
{
    public interface ITournamentTypeRepository
    {
        public Task<Ensuranx.Domain.Entities.TournamentType> GetTournamentTypeByNameAsync(string name);
    }
}
