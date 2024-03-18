using Ensuranx.Domain.Contracts;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Domain.Entities
{
    public class EmailTemplates : AuditableEntity<long>
    {
        public TemplatesEnum EmailTemplate { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
