using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers
{
    public class EventCollectionTeamMapper
    {
        public EventCollectionTeam MapEventCollectionTeamForCreate(string email,long teamId,long eventCollectionId)
        {
            EventCollectionTeam eventCollectionTeam = new EventCollectionTeam();

            eventCollectionTeam.EventCollectionId = eventCollectionId;
            eventCollectionTeam.TeamId = teamId;
            eventCollectionTeam.CreatedDateTime = DateTime.UtcNow;
            eventCollectionTeam.LastModifiedDateTime = DateTime.UtcNow;
            eventCollectionTeam.CreatedBy = email;
            eventCollectionTeam.LastModifiedBy = email;

            return eventCollectionTeam;
        }

        public List<EventCollectionTeam> MapEventCollectionTeamListForCreate(string email, List<long> teamId, long eventCollectionId)
        {
            List<EventCollectionTeam> eventCollectionTeamList = new List<EventCollectionTeam>();

            foreach (var item in teamId)
            {
                EventCollectionTeam eventCollectionTeam = new EventCollectionTeam();

                eventCollectionTeam.EventCollectionId = eventCollectionId;
                eventCollectionTeam.TeamId = item;
                eventCollectionTeam.CreatedDateTime = DateTime.UtcNow;
                eventCollectionTeam.LastModifiedDateTime = DateTime.UtcNow;
                eventCollectionTeam.CreatedBy = email;
                eventCollectionTeam.LastModifiedBy = email;
                eventCollectionTeam.Sequence = "A";

                eventCollectionTeamList.Add(eventCollectionTeam);
            }

            return eventCollectionTeamList;
        }
    }
}
