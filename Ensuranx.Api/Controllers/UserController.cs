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
using static System.Net.WebRequestMethods;
using Ensuranx.Api.Controllers;
using Ensuranx.Application.Contracts.Role.User;
using Ensuranx.Domain.Entities;
using Ensuranx.Application.Contracts.Providers;

namespace Ensuranx.Api.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ApiBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfigurationSection _jwtSettings;
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _mail;
        private readonly IUserService _iuserService;
        private readonly IUserInfoService _iuserInfoService;
      
        private readonly IHttpClientFactory _clientFactory;

        public UserController(UserManager<AppUser> userManager, IConfiguration configuration, 
                                ILogger<UserController> logger, ApplicationDbContext context, IEmailService mail, 
                                IHttpClientFactory clientFactory)
        {
            _userManager = userManager;
            _jwtSettings = configuration.GetSection("JWT");
            _logger = logger;
            _context = context;
            _mail = mail;
            _iuserService = new UserService(_mail, _userManager, _context, _logger, _jwtSettings);
            _iuserInfoService = new UserInfoService(_context, _logger, _userManager, _mail);
            _clientFactory = clientFactory;
            _clientFactory = clientFactory;
           
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            _logger.LogInformation("Register");
            ErrorOr<RegistrationResponse> registration = new ErrorOr<RegistrationResponse>();

            registration = await _iuserService.RegisterAsync(request);
            return registration.Match(
                registration => Ok(registration),
                errors => Problem(errors)
                );
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest userModel)
        {
            _logger.LogInformation("Login");

            UserService userService = new UserService(_mail, _userManager, _context, _logger, _jwtSettings);

            ErrorOr<LoginResponse> LoginResponse = await userService.LoginAsync(userModel);
            return LoginResponse.Match(
                LoginResponse => Ok(LoginResponse),
                errors => Problem(errors)
                );
        }

        [HttpPost("ResendOtp")]
        public async Task<IActionResult> ResendOtp(ResendOtpRequest resendOtpRequest)
        {
            _logger.LogInformation("VerifyOtpVerifyOtp");
            UserService userService = new UserService(_mail, _userManager, _context, _logger, _jwtSettings);
            ErrorOr<ResendOtpResponse> resendOtp = await userService.ResendOtp(resendOtpRequest);
            return resendOtp.Match(
                resendOtp => Ok(resendOtp),
                errors => Problem(errors)
            );
        }

        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            _logger.LogInformation("GetRoles");
            UserService userService = new UserService(_mail, _userManager, _context, _logger, _jwtSettings);
            ErrorOr<List<RoleResponse>> roleresponse = await userService.GetRolesAsync();
            return roleresponse.Match(
                roleresponse => Ok(roleresponse),
                errors => Problem(errors)
            );
        }

        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest verifyOtpRequest)
        {
            _logger.LogInformation("VerifyOtpVerifyOtp");

            var userId = this.UserID;
            var email = this.Email;

            ErrorOr<VerifyOtpResponse> verifyResponse = await _iuserInfoService.VerifyOtpService(verifyOtpRequest.OtpCode, verifyOtpRequest.Email);
            return verifyResponse.Match(
                verifyResponse => Ok(verifyResponse),
                errors => Problem(errors)
            );
        }

        [HttpPost("ForgetPasswordOtp")]
        public async Task<IActionResult> ForgetPasswordOtp(ForgetPasswordOtpRequest forgetPasswordRequest)
        {
            _logger.LogInformation("ForgetPassword");

            ErrorOr<ForgetPasswordResponse> forgetPasswordResponse = await _iuserInfoService.ForgetPasswordOtpAsync(forgetPasswordRequest);
            return forgetPasswordResponse.Match(
                forgetPasswordResponse => Ok(forgetPasswordResponse),
                errors => Problem(errors)
            );
        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest)
        {
            _logger.LogInformation("ForgetPassword");

            ErrorOr<ForgetPasswordResponse> forgetPasswordResponse = await _iuserInfoService.ForgetPasswordAsync(forgetPasswordRequest);
            return forgetPasswordResponse.Match(
                forgetPasswordResponse => Ok(forgetPasswordResponse),
                errors => Problem(errors)
            );
        }

        [HttpGet("AuthenticateUserDetails")]
        public async Task<IActionResult> AuthenticateUserDetails(ValidateUserDetails validateUserDetails)
        {
            _logger.LogInformation("AuthenticateUserDetails");

            UserService userService = new UserService(_mail, _userManager, _context, _logger, _jwtSettings);
            ErrorOr<ValidateUserDetailResponse> roleresponse = await userService.AuthenticateUserDetailsAsync(validateUserDetails);
            return roleresponse.Match(
                roleresponse => Ok(roleresponse),
                errors => Problem(errors)
            );
        }


        [HttpPost("DeleteUserAccount")]
        [Authorize]
        public async Task<IActionResult> DeleteUserAccount()
        {
            _logger.LogInformation("DeleteUserAccount");

            var userId = this.UserID;
            var email = this.Email;
            var roleId = this.RoleID;

            ErrorOr<GenericMessage> response = new ErrorOr<GenericMessage>();

            response = await _iuserInfoService.DeleteUserAccountAsync(userId, email, roleId);
            return response.Match(
                roleresponse => Ok(roleresponse),
                errors => Problem(errors));
        }



        [HttpGet("VerifyUserPassword")]
        [Authorize]
        public async Task<IActionResult> VerifyUserPassword(VerifyUserPassword verifyUserPassword)
        {
            _logger.LogInformation("PlayerSuggestion");

            var userId = this.UserID;
            var email = this.Email;
            var roleId = this.RoleID;

            ErrorOr<VerifyUserPasswordResponse> response = new ErrorOr<VerifyUserPasswordResponse>();

            response = await _iuserInfoService.VerifyUserPasswordAsync(userId, email, verifyUserPassword);
            return response.Match(
                roleresponse => Ok(roleresponse),
                errors => Problem(errors));
        }




    }
}

