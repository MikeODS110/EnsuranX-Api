using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface ITeamMemberRoleRepository
    {
        public Task<TeamMemberRole> GetTeamMemberRoleByName(string name);

        public Task<TeamMember> CheckMemberIsPresent(long teamId, long userId);

        public Task<WaitingList> GetWaitingListPlayer(long userId,long eventCollectionId);
    }
}
