using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IStatRepository
    {
        public Task<List<Stat>> GetActivityStatListAsync(long activityId);
    }
}
