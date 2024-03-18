using ErrorOr;
using FirebaseAdmin.Auth;
using Ensuranx.Application.Contracts;

using Ensuranx.Application.Contracts.UserInfo;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Requests.Statkeeper;
using Ensuranx.Application.Requests.UserInfo;
using Ensuranx.Application.Response.Role;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.Constants;
using Ensuranx.Common.Wrapper;
using Ensuranx.Common.WrapperInterface;
using Ensuranx.Domain.Common.Errors;
using Ensuranx.Domain.Entities;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Ensuranx.Infrastructure.Services.Device;
using Ensuranx.Infrastructure.Services.EmailTemplate;
using Ensuranx.Infrastructure.Services.Role;
using Ensuranx.Infrastructure.Services.UserInfo;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;
using Ensuranx.Application.Contracts.Role.User;
using Ensuranx.Application.Contracts.Device;

namespace Ensuranx.Infrastructure.Services.Identity
{
    /// <summary>
    /// all the actions related to user registration and login
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly IEmailService _mail;
        private readonly IConfigurationSection _jwtSettings;
        private readonly IUserInfoService _iuserInfoService;
        private readonly IUserRepository _iuserRepository;
        
        private IUnitOfWork<long> _iunitOfWork;
        private readonly IPermissionPermissionTypeRepository _ipermissionPermissionTypeRepository;
        private readonly IUserPermissionRepository _iuserPermissionRepository;
      

        public UserService(IEmailService mail, UserManager<AppUser> userManager, ApplicationDbContext context, ILogger logger, IConfigurationSection jwtSettings)
        {
            _mail = mail;
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _iunitOfWork = new UnitOfWork<long>(context);
            _jwtSettings = jwtSettings;
            _iuserInfoService = new UserInfoService(_context,_logger,_userManager,_mail);
            _iuserRepository = new UserRepository(context);
           
            _ipermissionPermissionTypeRepository = new PermissionPermissionTypeRepository(context);
            _iuserPermissionRepository = new UserPermissionRepository(context);
          
        }
        
        /// <summary>
        /// sign jwt token
        /// </summary>
        /// <returns>singing credientials of the user</returns>
        public SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.GetSection("Key").Value);
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// genrate jwt token for the user
        /// </summary>
        /// <param name="signingCredentials">signing user credientails</param>
        /// <param name="claims">claims for the user</param>
        /// <returns>jwt security token</returns>
        public JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
            issuer: _jwtSettings.GetSection("Issuer").Value,
            audience: _jwtSettings.GetSection("Audience").Value,
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettings.GetSection("expiryInMinutes").Value)),
            signingCredentials: signingCredentials);
            return tokenOptions;
        }


        /// <summary>
        /// get user claims
        /// </summary>
        /// <param name="user">user object</param>
        /// <returns>list of user claims</returns>
        public async Task<List<Claim>> GetClaims(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }

        /// <summary>
        /// get the user entity by email
        /// </summary>
        /// <param name="email">email of the user</param>
        /// <returns>user object</returns>
        public async Task<ErrorOr<AppUser>> GetUserByEmail(string email)
        {
            return await _userManager.Users.Where(x => x.Email == email).FirstOrDefaultAsync();
        }


        /// <summary>
        /// list of all the users
        /// </summary>
        /// <returns>list of user objects</returns>
        public async Task<IResult<List<AppUser>>> GetAllAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return await Result<List<AppUser>>.SuccessAsync(users);
        }

        /// <summary>
        /// register all the user roles to the system and send otp to email
        /// </summary>
        /// <param name="request">contains all the necessary information about the user</param>
        /// <returns>Id name email and token of the registered user</returns>
        public async Task<ErrorOr<RegistrationResponse>> RegisterAsync(RegistrationRequest request)
        {
            Notifications notifications = new Notifications();
            Ensuranx.Domain.Entities.NotificationTo notificationTo = new Ensuranx.Domain.Entities.NotificationTo();
      
            Ensuranx.Domain.Entities.UserInfo userInfo = new Ensuranx.Domain.Entities.UserInfo();
            UserInfoMapper userInfoMapper = new UserInfoMapper();
          

            MailMapper mailMapper = new MailMapper();
            Registration registration = new Registration();
            EmailTemplateService emailTemplateService = new EmailTemplateService(_context,_logger);

            try
            {
                ////if (await _userManager.FindByEmailAsync(request.Email) is not null)
                ////{
                ////    _logger.LogWarning("User is already present with this email {email}", request.Email);
                ////    return Errors.User.DuplicateEmail;
                ////}
                 if (await _userManager.FindByNameAsync(request.Username) is not null)
                {
                    _logger.LogWarning("User is already present with this username {username}", request.Username);
                    return Errors.User.DuplicateUsername;
                }
                else if (await _iuserRepository.CheckPhoneNumber(request.PhoneNumber))
                {
                    _logger.LogWarning("User is already present with this phonenumber {phonenumber}", request.PhoneNumber);
                    return Domain.Common.Errors.Errors.User.DuplicatePhoneNumber;
                }
                else
                {
                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        var user = new AppUser
                        {
                            Email = request.Email,
                            UserName = request.Username,
                            PhoneNumber = request.PhoneNumber,
                        };

                        _logger.LogInformation("Creating New User");
                        var result = await _userManager.CreateAsync(user, request.Password);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("New User {@user} is created", user);

                            try
                            {
                                _logger.LogInformation("Creating User Role");
                                await _userManager.AddToRoleAsync(user, request.RoleName);
                                _logger.LogInformation("User Role Created");
                            }
                            catch (Exception exc)
                            {
                                _logger.LogWarning("{exc}", exc);
                                return Errors.User.ExceptionMessage;
                            }

                            _logger.LogInformation("Assign Role to user", request.RoleName);
                            int OtpCode = createGuId();

                            userInfo = userInfoMapper.SetDataInUserInfo(user, OtpCode, request.Name, request.ZipCode);
                            _logger.LogInformation("Creating UserInfo");
                            userInfo = await _iuserInfoService.CreateUserInfo(userInfo);
                            _logger.LogInformation("{@userInfo} created", userInfo);

                            var emailTemp = await emailTemplateService.GetEmailByTemplateEnum(TemplatesEnum.SignupOtp);
                            if (emailTemp is not null)
                            {
                                var EmailReplaceBody = emailTemp.Body.Replace("@username", request.Name).Replace("@Otpcode", OtpCode.ToString());
                                //Email notification For sign up
                                string EmailSubject = emailTemp.Subject;
                                string EmailBody = OtpCode.ToString();

                                var mailData = mailMapper.SetDataInMailModel(user.Email, EmailSubject, EmailBody, Constants.APIErrorMessages.EMAIL_FROM);

                                _logger.LogInformation($"Send Email to {user.Email} with One-Time Password {OtpCode}");
                                bool EmailStatus = await _mail.SendAsync(mailData, new CancellationToken());
                                _logger.LogInformation($"Email Status is {EmailStatus}");

                                userInfo.OTPCode = OtpCode;
                                userInfo.IsSend = EmailStatus;

                                _logger.LogInformation($"Updating UserInfo");
                                userInfo = await _iuserInfoService.UpdateUserInfo(userInfo);
                                _logger.LogInformation("Updated {@userInfo}", userInfo);
                            }
                            else
                            {
                                _logger.LogError("Email template not found for SignupOtp");
                            }

                            _logger.LogInformation("User is Registered {@user}", user);

                            //default permission setup
                            bool defaultPermissionSetup = await CreateDefaultPermissionForUser(userInfo, user.Email);

                            var signingCredentials = GetSigningCredentials();
                            var claims = GetClaims(user);
                            var tokenOptions = GenerateTokenOptions(signingCredentials, await claims);
                            var Token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
                            string roleName = claims.Result.Where(c => c.Type == ClaimTypes.Role)
                                                        .Select(c => c.Value).FirstOrDefault();

                            TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

                            var config = TypeAdapterConfig.GlobalSettings;
                            TypeAdapterConfig<(AppUser User, string Token,string roleName), RegistrationResponse>.NewConfig()
                                .Map(dest => dest.Token, src => src.Token)
                                .Map(dest => dest.Role, src => src.roleName)
                                .Map(dest => dest, src => src.User);

                            RegistrationResponse registrationResponse = (user, Token,roleName).Adapt<RegistrationResponse>();

                            dbContextTransaction.Commit();
                            return registrationResponse;
                        }
                        else
                        {
                            _logger.LogError("User not created successfully {@result}", result);
                            return Errors.User.UserNotCreatedSuccessfully;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// verify user and generate jwt token for the user
        /// </summary>
        /// <param name="userModel">email and password of the user</param>
        /// <returns>Id name email and token of the registered user</returns>
        public async Task<ErrorOr<LoginResponse>> LoginAsync(LoginRequest userModel)
        {
            _logger.LogInformation($"Login request: {userModel}");

            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                _logger.LogInformation($"Looking for user {userModel.Email}");
                var user = await _userManager.FindByEmailAsync(userModel.Email);
                if (user is not null)
                {
                    _logger.LogInformation($"Going to fetch user info {userModel.Email}");
                    var userinfo = _iuserInfoService.GetUserInfoByUserIdAsync(user.Id);
                    if (userinfo != null)
                    {
                        _logger.LogInformation($"Going to verify user password {userModel.Email}");

                        if (await _userManager.CheckPasswordAsync(user, userModel.Password))
                        {
                            var signingCredentials = GetSigningCredentials();
                            var claims = GetClaims(user);
                            var tokenOptions = GenerateTokenOptions(signingCredentials, await claims);
                            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
                            string roleName = claims.Result.Where(c => c.Type == ClaimTypes.Role)
                                                        .Select(c => c.Value).FirstOrDefault();
                            bool isAddBusinessDetails = false;



                            //TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

                            //var config = TypeAdapterConfig.GlobalSettings;
                            //TypeAdapterConfig<(AppUser User, string Token, string roleName,
                            //     bool isLocked, bool isEmailVerified, long userInfoId
                            //    ), LoginResponse>.NewConfig()
                            //    .Map(dest => dest.Token, src => src.Token)
                            //    .Map(dest => dest, src => src.User)
                            //    .Map(dest => dest.Role, src => src.roleName)
                            //    .Map(dest => dest.IsLock, src => src.isLocked)
                            //    .Map(dest => dest.IsEmailVerified, src => src.isEmailVerified)
                            //    .Map(dest => dest.Id, src => src.userInfoId);

                            //LoginResponse loginResponse = (user, token, roleName, 
                            //     userinfo.IsEmailVerified, userinfo.Id
                            //    ).Adapt<LoginResponse>();
                            // Configure mapping
                            TypeAdapterConfig<(AppUser User, string Token, string roleName, bool isEmailVerified, long userInfoId), LoginResponse>
                                .NewConfig()
                                .Map(dest => dest.Token, src => src.Token)
                                .Map(dest => dest, src => src.User) // Map to User property of LoginResponse
                                .Map(dest => dest.Role, src => src.roleName)
                                .Map(dest => dest.IsEmailVerified, src => src.isEmailVerified)
                                .Map(dest => dest.Id, src => src.userInfoId);

                            // Create source tuple
                            (AppUser User, string Token, string roleName, bool isEmailVerified, long userInfoId) sourceTuple =
                                (user, token, roleName, userinfo.IsEmailVerified, userinfo.Id);

                            // Adapt the tuple to LoginResponse
                            LoginResponse loginResponse = sourceTuple.Adapt<LoginResponse>();


                            _logger.LogInformation("User successfully login Response {@loginResponse}", loginResponse);
                            return loginResponse;
                        }
                        else
                        {
                            _logger.LogWarning($"User Password is incorrect");
                            return Errors.Authentication.InvalidCredientials;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("UserInfo not found");
                        return Errors.Authentication.UserNotFound;
                    }
                }
                else
                {
                    _logger.LogWarning($"User Email is incorrect");
                    return Errors.Authentication.InvalidCredientials;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Errors.Authentication.ExceptionMessage;
            }
        }

        /// <summary>
        /// resend the otp to the user email or phonenumber
        /// </summary>
        /// <param name="resendOtpRequest">email or phone number</param>
        /// <returns>contains the success message or error problem</returns>
        public async Task<ErrorOr<ResendOtpResponse>> ResendOtp(ResendOtpRequest resendOtpRequest)
        {
            _logger.LogInformation("ResendOtp request: {@resendOtpRequest}", resendOtpRequest);
            MailMapper mailMapper = new MailMapper();

            EmailTemplateService emailTemplateService = new EmailTemplateService(_context, _logger);
            try
            {
                var user = await _userManager.FindByEmailAsync(resendOtpRequest.Email);

                if (user is not null)
                {
                    var userinfo = _iuserInfoService.GetUserInfoByUserIdAsync(user.Id);
                    int OtpCode = createGuId();

                    var emailTemp = await emailTemplateService.GetEmailByTemplateEnum(TemplatesEnum.ResendOtp);
                    if (emailTemp is not null)
                    {
                        var EmailReplaceBody = emailTemp.Body.Replace("@username", userinfo.Name).Replace("@otpcode", OtpCode.ToString());
                        //Email notification For sign up
                        string EmailSubject = emailTemp.Subject;
                        string EmailBody = EmailReplaceBody;

                        var mailData = mailMapper.SetDataInMailModel(user.Email, EmailSubject, EmailBody, Constants.APIErrorMessages.EMAIL_FROM);
                        _logger.LogInformation($"Send Email to {user.Email} with One-Time Password {OtpCode}");
                        bool EmailStatus = await _mail.SendAsync(mailData, new CancellationToken());
                        _logger.LogInformation($"Email Status is {EmailStatus}");

                        userinfo.OTPCode = OtpCode;
                        userinfo.IsSend = EmailStatus;

                        _logger.LogInformation($"Updating UserInfo");
                        userinfo = await _iuserInfoService.UpdateUserInfo(userinfo);
                        _logger.LogInformation("Updated {@userInfo}", userinfo);

                        _logger.LogInformation("Email Send Successfully");
                        return new ResendOtpResponse { Message = Constants.APIErrorMessages.RESEND_EMAIL };
                    }
                    else
                    {
                        return Errors.Authentication.EmailTemplateNotFound;
                    }
                }
                else
                {
                    return Errors.Authentication.UserNotFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Errors.Authentication.ExceptionMessage;
            }
        }
        
        /// <summary>
        /// generate unique otp-code
        /// </summary>
        /// <returns>6 digit otp code</returns>
        public int createGuId()
        {
            _logger.LogInformation("Generating One-Time Password");
            Random rand = new Random((int)DateTime.Now.Ticks);
            int numIterations = rand.Next(100000, 999999);
            _logger.LogInformation($"One-Time Password is ready {numIterations}");
            return numIterations;
        }
        /// <summary>
        /// get all system roles
        /// </summary>
        /// <returns>id name and description for system roles</returns>
        public async Task<ErrorOr<List<RoleResponse>>> GetRolesAsync()
        {
            RoleService roleService = new RoleService(_context,_userManager);
            try
            {
                _logger.LogInformation("Going to fetch roles");
                var roles = await roleService.GetRolesList();
                List<RoleResponse> roleResponse = roles.Adapt<List<RoleResponse>>();
                _logger.LogInformation("Roles fetch successfully {@roleResponse}", roleResponse);
                return roleResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Errors.Authentication.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<ValidateUserDetailResponse>> AuthenticateUserDetailsAsync(ValidateUserDetails validateUserDetails)
        {
            try
            {
                if (!string.IsNullOrEmpty(validateUserDetails.Email) && await _userManager.FindByEmailAsync(validateUserDetails.Email) is not null)
                {
                    _logger.LogWarning("User is already present with this email {email}", validateUserDetails.Email);
                    return Errors.User.DuplicateEmail;
                }
                else if (!string.IsNullOrEmpty(validateUserDetails.UserName) && await _userManager.FindByNameAsync(validateUserDetails.UserName) is not null)
                {
                    _logger.LogWarning("User is already present with this username {username}", validateUserDetails.UserName);
                    return Errors.User.DuplicateUsername;
                }
                else if (!string.IsNullOrEmpty(validateUserDetails.PhoneNumber) && await _iuserRepository.CheckPhoneNumber(validateUserDetails.PhoneNumber))
                {
                    _logger.LogWarning("User is already present with this phonenumber {phonenumber}", validateUserDetails.PhoneNumber);
                    return Domain.Common.Errors.Errors.User.DuplicatePhoneNumber;
                }
                else
                {
                    return new ValidateUserDetailResponse();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Errors.Authentication.ExceptionMessage;
            }
        }
        
        

        /// <summary>
        /// add default permission for new users
        /// </summary>
        /// <param name="userInfo">user which is signing up</param>
        /// <param name="email">user email who is login</param>
        /// <returns></returns>
        public async Task<bool> CreateDefaultPermissionForUser(Domain.Entities.UserInfo userInfo, string email)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            PermissionMapper permissionMapper = new PermissionMapper();

            List<Domain.Entities.Permission> permissionList = await _iunitOfWork.Repository<Domain.Entities.Permission>().GetAllAsync();
            List<long> permissionIdList = permissionList.Select(x => x.Id).ToList();
            Domain.Entities.PermissionType permissionType = await _ipermissionPermissionTypeRepository.GetPermissionTypeByPermissionTypeName(Domain.Enums.Enum.PermissionType.Everyone.ToString());
            List<Domain.Entities.PermissionPermissionType> permissionPermissionTypeList = await _ipermissionPermissionTypeRepository.GetPermissionPermissionTypeListByPermissionIdListAndPermissionTypeId(permissionIdList, permissionType.Id);
            List<long> permissionPermissionTypeIdList = permissionPermissionTypeList.Select(x => x.Id).ToList();
            List<UserPermission> userPermissionList = permissionMapper.MapUserWithPermissionPermissionTypeId(userInfo.Id,email, permissionPermissionTypeIdList);

            await _iuserPermissionRepository.CreateUserPermissionAsync(userPermissionList);
            _logger.LogInformation("default permission assign to user successfully {@userPermissionList}", userPermissionList);

            return true;
        }
    }
}
