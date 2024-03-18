using Ensuranx.Domain.Entities;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Application.Interfaces
{
    public interface IEmailTemplateRepository
    {
        public Task<EmailTemplates> GetEmailByTemplateEnum(TemplatesEnum templatesEnum);
    }
}
