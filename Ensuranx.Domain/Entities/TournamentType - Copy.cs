using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities;

public class TournamentType : AuditableEntity<long>
{
    public string Name { get; set; } = string.Empty;
}
