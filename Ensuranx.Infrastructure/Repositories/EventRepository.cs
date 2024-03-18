using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
 
using System.Reflection.Metadata.Ecma335;

namespace Ensuranx.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EventType>> GetEventTypeAsync()
        {
            return await _context.EventType.AsNoTracking().ToListAsync();
        }

        public async Task<EventStatus> GetEventStatusByNameAsync(string name)
        {
            return await _context.EventStatus.AsNoTracking().Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<EventTeam>> CreateEventTeamList(List<EventTeam> eventTeamList)
        {
            await _context.EventTeam.AddRangeAsync(eventTeamList);
            await _context.SaveChangesAsync();
            return eventTeamList;
        }

        public async Task<DateTime> GetLastPlayedDateTimeForUserByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.Event)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId

                               where teamMemberTbl.PlayerId == userId && 
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower() &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower()

                               orderby eventTeamTbl.Id descending

                               select eventTeamTbl.Event.EventDateTime
                               ).FirstOrDefaultAsync();
            return query;
        }

        public async Task<DateTime> GetLastMvpDateTimeForUserByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.Event)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId
                               join eventTeamMemberStatResultTbl in _context.EventTeamMemberStatResult.AsNoTracking().Include(e => e.Event)
                               on teamMemberTbl.PlayerId equals eventTeamMemberStatResultTbl.TeamMember.PlayerId

                               where teamMemberTbl.PlayerId == userId &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower() &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() &&
                               eventTeamMemberStatResultTbl.IsMvp != false

                               orderby eventTeamMemberStatResultTbl.Id descending

                               select eventTeamMemberStatResultTbl.Event.EventDateTime
                               ).FirstOrDefaultAsync();
            return query;
        }

        public async Task<int> GetMatchWinCountByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.Event)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId
                               

                               where teamMemberTbl.PlayerId == userId &&
                               eventTeamTbl.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()
                               
                               select eventTeamTbl.Event.Id
                               ).CountAsync();
            return query;
        }

        public async Task<int> GetMatchLoseCountByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.Event)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId


                               where teamMemberTbl.PlayerId == userId &&
                               eventTeamTbl.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower()

                               select eventTeamTbl.Event.Id
                               ).CountAsync();
            return query;
        }

        public async Task<int> GetMatchTieCountByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.Event)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId


                               where teamMemberTbl.PlayerId == userId &&
                               eventTeamTbl.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower()

                               select eventTeamTbl.Event.Id
                               ).CountAsync();
            return query;
        }

        public async Task<int> GetOfficialTeamCountByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id

                               where teamMemberTbl.PlayerId == userId && teamTbl.TeamType.Name.ToLower() != Domain.Enums.Enum.TeamType.Temporary.ToString().ToLower()
                               select teamTbl.Id
                               ).CountAsync();
            return query;
        }

        public async Task<int> GetTemporaryTeamCountByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id

                               where teamMemberTbl.PlayerId == userId && teamTbl.TeamType.Name.ToLower() != Domain.Enums.Enum.TeamType.Official.ToString().ToLower()
                               select teamTbl.Id
                               ).CountAsync();
            return query;
        }

        public async Task<int> GetVenuesCountInWhichUserPlayedByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.Event)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId
                               join eventCollectionTbl in _context.EventCollection.AsNoTracking()
                               on eventCollectionTeamTbl.EventCollectionId equals eventCollectionTbl.Id

                               where teamMemberTbl.PlayerId == userId &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower()

                               group eventCollectionTbl by eventCollectionTbl.VenueId into VenueGrouped

                               select VenueGrouped.Key
                               ).CountAsync();
            return query;
        }

        public async Task<int> GetTournamentCountInWhichUserPlayedByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.Event)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId
                               join eventCollectionTbl in _context.EventCollection.AsNoTracking()
                               on eventCollectionTeamTbl.EventCollectionId equals eventCollectionTbl.Id
                               join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                               on eventCollectionTbl.Id equals tournamentEventCollectionTbl.EventCollectionId

                               where teamMemberTbl.PlayerId == userId &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower()

                               group tournamentEventCollectionTbl by tournamentEventCollectionTbl.TournamentId into tournamentGrouped

                               select tournamentGrouped.Key
                               ).CountAsync();
            return query;
        }

        public async Task<int> GetTournamentWinCountInWhichUserPlayedByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.Event)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId
                               join eventCollectionTbl in _context.EventCollection.AsNoTracking()
                               on eventCollectionTeamTbl.EventCollectionId equals eventCollectionTbl.Id
                               join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                               on eventCollectionTbl.Id equals tournamentEventCollectionTbl.EventCollectionId
                               join tournamentResultTbl in _context.TournamentResult.AsNoTracking()
                               on new { tournamentEventCollectionTbl.TournamentId, eventCollectionTeamTbl.TeamId } equals new 
                               { tournamentResultTbl.TournamentId, tournamentResultTbl.TeamId }
                               where teamMemberTbl.PlayerId == userId &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower() &&
                               tournamentResultTbl.Position.Name.ToLower() == Domain.Enums.Enum.Position.First.ToString().ToLower()

                               group tournamentEventCollectionTbl by tournamentEventCollectionTbl.TournamentId into tournamentGrouped

                               select tournamentGrouped.Key
                               ).CountAsync();
            return query;
        }

        public async Task<int> GetTournamentLoseCountInWhichUserPlayedByUserIdAsync(long userId)
        {
            var query = await (from teamMemberTbl in _context.TeamMember.AsNoTracking()
                               join teamTbl in _context.Team.AsNoTracking()
                               on teamMemberTbl.TeamId equals teamTbl.Id
                               join eventCollectionTeamTbl in _context.EventCollectionTeam.AsNoTracking()
                               on teamTbl.Id equals eventCollectionTeamTbl.TeamId
                               join eventTeamTbl in _context.EventTeam.AsNoTracking().Include(x => x.EventStatus).Include(x => x.Event)
                               on eventCollectionTeamTbl.Id equals eventTeamTbl.EventCollectionTeamId
                               join eventCollectionTbl in _context.EventCollection.AsNoTracking()
                               on eventCollectionTeamTbl.EventCollectionId equals eventCollectionTbl.Id
                               join tournamentEventCollectionTbl in _context.TournamentEventCollection.AsNoTracking()
                               on eventCollectionTbl.Id equals tournamentEventCollectionTbl.EventCollectionId
                               join tournamentResultTbl in _context.TournamentResult.AsNoTracking()
                               on new { tournamentEventCollectionTbl.TournamentId, eventCollectionTeamTbl.TeamId } equals new { tournamentResultTbl.TournamentId, tournamentResultTbl.TeamId }
                               where teamMemberTbl.PlayerId == userId &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() &&
                               eventTeamTbl.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.InLobby.ToString().ToLower() &&
                               tournamentResultTbl.Position.Name.ToLower() != Domain.Enums.Enum.Position.First.ToString().ToLower()

                               group tournamentEventCollectionTbl by tournamentEventCollectionTbl.TournamentId into tournamentGrouped

                               select tournamentGrouped.Key
                               ).CountAsync();
            return query;
        }

        public async Task<int> GetActivityTime(long eventId)
        {
            var results = await (from activity in _context.Activity
                                 join ec in _context.EventCollection


                                 on activity.Id equals ec.ActivityId

                                 join ect in _context.EventCollectionTeam
                                 on ec.Id equals ect.EventCollectionId

                                 join evenTeam in _context.EventTeam
                                 on ect.Id equals evenTeam.EventCollectionTeamId

                                 join events in _context.Event
                                 on evenTeam.EventId equals events.Id

                                 where events.Id == eventId

                                 select activity.MinutesOfActivity).FirstOrDefaultAsync();
                                   



            return (int)results;
        }
        public async Task<long> GetQuaters(long eventId)
        {
            var results = await (from activity in _context.Activity
                                join ec in _context.EventCollection


                                on activity.Id equals ec.ActivityId

                                join ect in _context.EventCollectionTeam
                                on ec.Id equals ect.EventCollectionId

                                join evenTeam in _context.EventTeam
                                on ect.Id equals evenTeam.EventCollectionTeamId

                                join events in _context.Event
                                on evenTeam.EventId equals events.Id

                                where events.Id == eventId

                                select activity.Quarters).FirstOrDefaultAsync();




            return results;
        }
    }
}
