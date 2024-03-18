using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class EventTeamMapper
    {
        public List<EventTeam> CreateEventTeamList(string email, List<EventCollectionTeam> eventCollectionTeamList,long eventId,long eventStatusId,int i)
        {
            List<EventTeam> eventTeamList = new List<EventTeam>();

            foreach (var eventCollectionTeam in eventCollectionTeamList)
            {
                EventTeam eventTeam = new EventTeam();

                eventTeam.EventId = eventId;
                eventTeam.EventStatusId = eventStatusId;
                eventTeam.EventCollectionTeamId = eventCollectionTeam.Id;
                eventTeam.CreatedDateTime = DateTime.UtcNow;
                eventTeam.LastModifiedDateTime = DateTime.UtcNow;
                eventTeam.CreatedBy = email;
                eventTeam.LastModifiedBy = email;
                eventTeam.Group = i < 4 ? 'A' : i < 8 ? 'B' : i < 12 ? 'C' : i < 16 ? 'D' : null;

                eventTeamList.Add(eventTeam);
            }
            return eventTeamList;
        }

        public List<EventTeam> CreateEventTeamSeasonList(string email, List<EventCollectionTeam> eventCollectionTeamList, long eventId, long eventStatusId)
        {
            List<EventTeam> eventTeamList = new List<EventTeam>();

            foreach (var eventCollectionTeam in eventCollectionTeamList)
            {
                EventTeam eventTeam = new EventTeam();
                eventTeam.EventId = eventId;
                eventTeam.EventStatusId = eventStatusId;
                eventTeam.EventCollectionTeamId = eventCollectionTeam.Id;
                eventTeam.CreatedDateTime = DateTime.UtcNow;
                eventTeam.LastModifiedDateTime = DateTime.UtcNow;
                eventTeam.CreatedBy = email;
                eventTeam.LastModifiedBy = email;
                eventTeam.Round = -1;
                eventTeamList.Add(eventTeam);
            }
            return eventTeamList;
        }

        public List<EventTeam> MapEventTeamStatus(List<EventTeam> eventTeamList,EventStatus eventStatus)
        {
            foreach (EventTeam eventTeam in eventTeamList)
            {
                eventTeam.EventStatusId = eventStatus.Id;
                eventTeam.LastModifiedDateTime = DateTime.UtcNow;
            }
            return eventTeamList;
        }
    }
}
