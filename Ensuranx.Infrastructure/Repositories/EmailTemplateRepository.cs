using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Repositories
{
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmailTemplates> GetEmailByTemplateEnum(TemplatesEnum templatesEnum)
        {
            return await _context.EmailTemplate.AsNoTracking().Where(x => x.EmailTemplate == templatesEnum).FirstOrDefaultAsync();
        }
    }
}
