using Ensuranx.Domain.Entities;

namespace Ensuranx.Application.Interfaces
{
    public interface IEventRepository
    {
        public Task<List<EventType>> GetEventTypeAsync();
        public Task<EventStatus> GetEventStatusByNameAsync(string name);
        public Task<List<EventTeam>> CreateEventTeamList(List<EventTeam> eventTeamList);
        public Task<DateTime> GetLastPlayedDateTimeForUserByUserIdAsync(long userId);
        public Task<DateTime> GetLastMvpDateTimeForUserByUserIdAsync(long userId);
        public Task<int> GetMatchWinCountByUserIdAsync(long userId);
        public Task<int> GetMatchLoseCountByUserIdAsync(long userId);
        public Task<int> GetMatchTieCountByUserIdAsync(long userId);
        public Task<int> GetOfficialTeamCountByUserIdAsync(long userId);
        public Task<int> GetTemporaryTeamCountByUserIdAsync(long userId);
        public Task<int> GetVenuesCountInWhichUserPlayedByUserIdAsync(long userId);
        public Task<int> GetTournamentCountInWhichUserPlayedByUserIdAsync(long userId);
        public Task<int> GetTournamentWinCountInWhichUserPlayedByUserIdAsync(long userId);
        public Task<int> GetTournamentLoseCountInWhichUserPlayedByUserIdAsync(long userId);

        public Task<int> GetActivityTime(long eventId);
        public Task<long> GetQuaters(long eventId);
    }
}
