using Ensuranx.Domain.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ensuranx.Domain.Entities
{
    public class Tournament : AuditableEntity<long>
    {
        public string Name { get; set; }
        public virtual long ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; }
        public virtual long TournamentTypeId { get; set; }
        [ForeignKey("TournamentTypeId")]
        public virtual TournamentType TournamentType { get; set; }
        public  DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime SeasonStartDate { get; set; }
        public DateTime SeasonEndDate { get;set; }
        public long? FirstPrize { get; set; }
        public long? SecondPrize { get; set; }
        public long? ThirdPrize { get; set; }
        public int TeamCapacity { get; set; }
        public int SeasonTeamCapacity { get; set; }
        public int MinPlayerPerTeam { get; set; }
        public int MaxPlayerPerTeam { get; set; }
    }
}
