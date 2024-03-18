using Ensuranx.Domain.Entities;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Application.Contracts.EmailTemplate
{
    public interface IEmailTemplateService
    {
        public Task<EmailTemplates> GetEmailByTemplateEnum(TemplatesEnum templatesEnum);
    }
}
