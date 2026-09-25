using Azure;
using ErrorOr;
using Ensuranx.Api.Middlewares;
using System.Net.Http;
using Ensuranx.Application.Contracts;
using Ensuranx.Application.Contracts.UserInfo;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Requests.UserInfo;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Role;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Services.Identity;
using Ensuranx.Infrastructure.Services.UserInfo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Azure;
using ErrorOr;
using Ensuranx.Api.Middlewares;
using System.Net.Http;
using Ensuranx.Application.Contracts;
using Ensuranx.Application.Contracts.UserInfo;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Requests.UserInfo;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Role;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Services.Identity;
using Ensuranx.Infrastructure.Services.UserInfo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection.Emit;
using Ensuranx.Api.Controllers;
using System.Runtime.CompilerServices;
using Ensuranx.Application.Contracts.Providers;
using Ensuranx.Infrastructure.Services.Provider;
using Ensuranx.Domain.Entities;

namespace Ensuranx.Api.Controllers
{
    [Route("api/[controller]")]
   
    public class ProviderController : ApiBaseController
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        private readonly ILogger<ProviderController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IProviderService _provider;

        public ProviderController(UserManager<AppUser> userManager, ILogger<ProviderController> logger, ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
            _provider = new ProviderService(context, logger);
        }

        [HttpGet("GetAllProviders")]
        [Authorize(Roles =("Admin"))]
        public async Task<IActionResult> GetAllProviders()
        {
            try
            {
                var providers = await _provider.GetAllProvider();
                return Ok(providers);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

    }
}
