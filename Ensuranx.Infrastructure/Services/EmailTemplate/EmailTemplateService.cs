using Ensuranx.Application.Contracts.EmailTemplate;
using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Services.EmailTemplate
{
    /// <summary>
    /// contains action related to email template table
    /// </summary>
    public class EmailTemplateService : IEmailTemplateService
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private IUnitOfWork<long> _iunitOfWork;
        private IEmailTemplateRepository _emailTemplateRepository;

        public EmailTemplateService(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
            _iunitOfWork = new UnitOfWork<long>(context);
            _emailTemplateRepository = new EmailTemplateRepository(_context);
        }

        /// <summary>
        /// get email template from template enum
        /// </summary>
        /// <param name="templatesEnum">contains all possible emails generating scenerios</param>
        /// <returns>email template entity</returns>
        public async Task<EmailTemplates> GetEmailByTemplateEnum(TemplatesEnum templatesEnum)
        {
            return await _emailTemplateRepository.GetEmailByTemplateEnum(templatesEnum);
        }
    }
}
