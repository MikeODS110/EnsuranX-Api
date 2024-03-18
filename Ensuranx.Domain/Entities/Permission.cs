using Ensuranx.Domain.Contracts;

namespace Ensuranx.Domain.Entities;

public class Permission : AuditableEntity<long>
{
    public string Name { get; set; } = default!;
}

