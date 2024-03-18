using Ensuranx.Domain.Entities;

namespace Ensuranx.Infrastructure.Mappers;

public class TournamentEventCollectionMapper
{
    public List<TournamentEventCollection> MapEventCollectionWithTournamentList(List<EventCollection> eventCollectionList,long tournamentId,string email)
    {
        List<TournamentEventCollection> tournamentEventCollectionList = new List<TournamentEventCollection>();

        foreach (EventCollection eventCollection in eventCollectionList)
        {
            TournamentEventCollection tournamentEventCollection = new TournamentEventCollection();

            tournamentEventCollection.TournamentId = tournamentId;
            tournamentEventCollection.EventCollectionId = eventCollection.Id;
            tournamentEventCollection.CreatedBy = email;
            tournamentEventCollection.LastModifiedBy = email;
            tournamentEventCollection.CreatedDateTime = DateTime.UtcNow;
            tournamentEventCollection.LastModifiedDateTime = DateTime.UtcNow;

            tournamentEventCollectionList.Add(tournamentEventCollection);
        }

        return tournamentEventCollectionList;
    }

    public TournamentEventCollection MapEventCollectionWithTournament(EventCollection eventCollection, long tournamentId, string email)
    {
        TournamentEventCollection tournamentEventCollection = new TournamentEventCollection();

        tournamentEventCollection.TournamentId = tournamentId;
        tournamentEventCollection.EventCollectionId = eventCollection.Id;
        tournamentEventCollection.CreatedBy = email;
        tournamentEventCollection.LastModifiedBy = email;
        tournamentEventCollection.CreatedDateTime = DateTime.UtcNow;
        tournamentEventCollection.LastModifiedDateTime = DateTime.UtcNow;


        return tournamentEventCollection;
    }
}
