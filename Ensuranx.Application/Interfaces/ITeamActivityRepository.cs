using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces;

public interface ITeamActivityRepository
{
    public Task<TeamActivity> GetTeamActivityByTeamId(long teamId);
}
