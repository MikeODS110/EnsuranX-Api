using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities;

public class Position : AuditableEntity<long>
{
    public string Name { get; set; } = default!;
}
