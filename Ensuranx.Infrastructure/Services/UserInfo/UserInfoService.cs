using ErrorOr;
using Firebase.Storage;
using FirebaseAdmin.Auth;
using Ensuranx.Application.Contracts;
using Ensuranx.Application.Contracts.EmailTemplate;
using Ensuranx.Application.Contracts.UserInfo;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Requests.Profile;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Common.Errors;
using Ensuranx.Domain.Entities;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Ensuranx.Infrastructure.Services.EmailTemplate;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;
using static System.Net.WebRequestMethods;

namespace Ensuranx.Infrastructure.Services.UserInfo
{
    public class UserInfoService : IUserInfoService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailTemplateService _iemailTemplateService;
        private readonly IConnectionRepository _iconnectionRepository;
        private readonly IEmailService _mail;
        private readonly IConnectionEntityRepository _iconnectionEntityRepository;
        private readonly IUserPermissionRepository _iuserPermissionRepository;
        private readonly IUserInfoService _iuserInfoService;
        private readonly IPermissionPermissionTypeRepository _ipermissionPermissionTypeRepository;
        private readonly IImagesRepository _iimagesRepository;


        public UserInfoService(ApplicationDbContext context, ILogger logger, UserManager<AppUser> userManager, IEmailService mail)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _context = context;
            _userInfoRepository = new UserInfoRepository(context);
            _userManager = userManager;
            _iimagesRepository = new ImagesRepository(context);
            _iemailTemplateService = new EmailTemplateService(context, _logger);
            _mail = mail;
            _iconnectionRepository = new ConnectionRepository(context);
            _iconnectionEntityRepository = new ConnectionEntityRepository(context);
            _iuserPermissionRepository = new UserPermissionRepository(context);
            _ipermissionPermissionTypeRepository = new PermissionPermissionTypeRepository(context);
        }

        public async Task<Ensuranx.Domain.Entities.UserInfo> CreateUserInfo(Ensuranx.Domain.Entities.UserInfo userInfo)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            var result = await _iunitOfWork.Repository<Ensuranx.Domain.Entities.UserInfo>().AddAsync(userInfo);
            await _iunitOfWork.Commit(cancellationToken);
            return result;
        }

        public async Task<Ensuranx.Domain.Entities.UserInfo> UpdateUserInfo(Ensuranx.Domain.Entities.UserInfo userInfo)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            await _iunitOfWork.Repository<Ensuranx.Domain.Entities.UserInfo>().UpdateAsync(userInfo);
            await _iunitOfWork.Commit(cancellationToken);
            return userInfo;
        }

        public async Task<Ensuranx.Domain.Entities.UserInfo> GetUserInfoById(long userInfoId)
        {
            return await _iunitOfWork.Repository<Ensuranx.Domain.Entities.UserInfo>().GetByIdAsync(userInfoId);
        }

        public Ensuranx.Domain.Entities.UserInfo GetUserInfoByUserId(long userId)
        {
            return _userInfoRepository.GetUserInfoByUserId(userId);
        }
        

        public Ensuranx.Domain.Entities.UserInfo GetUserInfoByUserIdAsync(int userId)
        {
            return _userInfoRepository.GetUserInfoByUserIdAsync(userId);
        }

        public async Task<ErrorOr<VerifyOtpResponse>> VerifyOtpService(int otpCode, string email)
        {
            _logger.LogInformation("VerifyOtpService with otp {@otpcode} and user {@email}", otpCode, email);
            var userinfo = _userInfoRepository.CheckForValidOtpCode(otpCode, email);

            if (userinfo is not null)
            {
                _logger.LogInformation("Updating Userinfo status to isemailverified true");
                userinfo.IsEmailVerified = true;
                await UpdateUserInfo(userinfo);
                _logger.LogInformation("Userinfo updated {@userinfo}", userinfo);
            }
            else
            {
                _logger.LogInformation("Userinfo not found or invalid otpCode");
                return Domain.Common.Errors.Errors.User.ErrorInvalidOtpCode;
            }

            _logger.LogInformation("Email verified Successfully");
            return new VerifyOtpResponse { Message = Constants.APIErrorMessages.VERIFIED_EMAIL };
        }


        public async Task<string> AddingImageToFirebase(MemoryStream memoryStream, IConfiguration _firebasePath)
        {
            Guid guid = Guid.NewGuid();
            var SubTask = new FirebaseStorage(_firebasePath.GetSection("Path").Value)
                            .Child(_firebasePath.GetSection("UserFolder").Value)
                            .Child(DateTime.UtcNow.Date.Second.ToString())
                            .PutAsync(memoryStream);

            var SubdownloadUrl = await SubTask;
            return SubdownloadUrl;
        }

        public MemoryStream CovertingByteToMemoryStream(string image)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(image);
            using (MemoryStream fileStream = new MemoryStream(bytes))
            {
                return fileStream;
            }
        }

        public async Task<ErrorOr<GenericMessage>> SaveProfileDetails(AddProfileDetails addProfileDetails, long userId, IConfiguration _firebasePath)
        {
            _logger.LogInformation("SaveProfileDetails request model {@addProfileDetails}", addProfileDetails);
            Domain.Entities.UserInfo userInfo = new Domain.Entities.UserInfo();

            var Profile = Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
            var Cover = Constants.APIErrorMessages.USER_DEFAULT_COVER_IMAGE;

            try
            {
                userInfo = _userInfoRepository.GetUserInfoByUserId(userId);
                var currentUser = userInfo.User;

                Domain.Entities.ConnectionEntity connectionEntity = await _iconnectionEntityRepository.GetConnectionEntityAsync(Domain.Enums.Enum.ConnectionEntity.User.ToString());
                var imageTypeList = await _iunitOfWork.Repository<Domain.Entities.ImageType>().GetAllAsync();

                long profileImageType = imageTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();
                long coverImageType = imageTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ImageType.Cover.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                if (!string.IsNullOrEmpty(addProfileDetails.Email) && currentUser.Email.ToLower() != addProfileDetails.Email.ToLower())
                    currentUser.Email = addProfileDetails.Email;

                if (!string.IsNullOrEmpty(addProfileDetails.UserName) && currentUser.UserName.ToLower() != addProfileDetails.UserName.ToLower())
                    currentUser.UserName = addProfileDetails.UserName;

                if (!string.IsNullOrEmpty(addProfileDetails.PhoneNumber))
                    currentUser.PhoneNumber = addProfileDetails.PhoneNumber;


           

                if (!string.IsNullOrEmpty(addProfileDetails.PhoneNumber) && _userManager.Users.Any(item => item.PhoneNumber == addProfileDetails.PhoneNumber))
                    return Errors.User.DuplicatePhoneNumber;

                if (!string.IsNullOrEmpty(addProfileDetails.Name))
                    userInfo.Name = addProfileDetails.Name;

                {
                    _logger.LogInformation($"Cover Image {addProfileDetails.CoverImage}");
                  
                   
                        _logger.LogInformation("creating user image");
                        var img = new Domain.Entities.Images
                        {
                            ImageTypeId = profileImageType,
                            CreatedBy = currentUser.Email,
                            CreatedDateTime = DateTime.UtcNow,
                            LastModifiedDateTime = DateTime.UtcNow,
                            LastModifiedBy = currentUser.Email,
                            ConnectionEntityId = connectionEntity.Id,
                            Path = addProfileDetails.CoverImage,
                            TableId = userInfo.Id
                        };
                        img = await _iimagesRepository.CreateImageAsync(img);
                        Cover = addProfileDetails.CoverImage;
                        _logger.LogInformation("Images updated: {@img}", img);
                    
                }

                IdentityResult result = await _userManager.UpdateAsync(currentUser);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User Updated Successfully: {@result}", result);
                    userInfo = _userInfoRepository.UpdateUserInfo(userInfo);
                    _logger.LogInformation("UserInfo Updated Successfully: {@userInfo}", userInfo);
                    /*TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

                    var config = TypeAdapterConfig.GlobalSettings;
                    TypeAdapterConfig<(AppUser User, string Name, string ProfileImage, string CoverImage), ProfileDetailsResponse>.NewConfig()
                        .Map(dest => dest.Name, src => src.Name)
                        .Map(dest => dest.ProfileImageUrl, src => src.ProfileImage)
                        .Map(dest => dest.CoverImageUrl, src => src.CoverImage)
                        .Map(dest => dest, src => src.User);
                    ProfileDetailsResponse profileDetailsResponse = (currentUser, userInfo.Name, Profile, Cover).Adapt<ProfileDetailsResponse>();
                    _logger.LogInformation("profileDetailsResponse: {@profileDetailsResponse}", profileDetailsResponse);*/
                    return new GenericMessage { Message = Constants.APIErrorMessages.RECORD_UPDATED};
                }
                else
                {
                    _logger.LogError("User not Updated: {@result}", result);
                    Error errror = Error.Validation(code: "User." + result.Errors.FirstOrDefault().Code + "", description: result.Errors.FirstOrDefault().Description);
                    return errror;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<PasswordUpdatedResponse>> UpdatePasswordAsync(UpdatePassword updatePassword, long userId)
        {
            _logger.LogInformation("UpdatePasswordAsync {@updatePassword}", updatePassword);

            try
            {
                var user = _userInfoRepository.GetUserInfoByUserId(userId);
                _logger.LogInformation("User is {@user}", user.User);
                IdentityResult result = await _userManager.ChangePasswordAsync(user.User, updatePassword.OldPassword, updatePassword.NewPassword);
                _logger.LogInformation("Update password response {@result}", result);
                return result.Succeeded ? new PasswordUpdatedResponse { Message = Constants.APIErrorMessages.PASSWORD_UPDATED_SUCESSFULLY } : result.Errors.FirstOrDefault().Code.ToLower() == "passwordmismatch" ? Errors.User.ErrorOldPasswordMisMatched : Errors.User.ErrorUserPasswordUpdate;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<ForgetPasswordResponse>> ForgetPasswordOtpAsync(ForgetPasswordOtpRequest forgetPasswordRequest)
        {

            _logger.LogInformation("ForgetPasswordAsync");
            Domain.Entities.UserInfo userInfo = new Domain.Entities.UserInfo();
            EmailTemplateService emailTemplateService = new EmailTemplateService(_context, _logger);

            MailMapper mailMapper = new MailMapper();
            try
            {
                var user = await _userManager.FindByEmailAsync(forgetPasswordRequest.Email);
                if (user is null)
                {
                    _logger.LogWarning($"Couldn't found user with this email {forgetPasswordRequest.Email}");
                    return Errors.User.UserNotFoundWithThisEmail;
                }
                else
                {
                    int OtpCode = createGuId();
                    userInfo = _userInfoRepository.GetUserInfoByUserTableId(user.Id);
                    if (userInfo is not null)
                    {


                        userInfo.OTPCode = OtpCode;
                        var updateotp = _userInfoRepository.UpdateUserInfo(userInfo);
                        var emailTemp = await emailTemplateService.GetEmailByTemplateEnum(TemplatesEnum.ForgetPassword);
                        if (emailTemp is not null)
                        {
                            var EmailReplaceBody = emailTemp.Body.Replace("@username", userInfo.Name).Replace("@otpcode", OtpCode.ToString());
                            //Email notification For sign up
                            string EmailSubject = emailTemp.Subject;
                            string EmailBody = OtpCode.ToString();

                            var mailData = mailMapper.SetDataInMailModel(user.Email, EmailSubject, EmailBody, Constants.APIErrorMessages.EMAIL_FROM);

                            _logger.LogInformation($"Send Email to {user.Email} with One-Time Password {OtpCode}");
                            bool EmailStatus = await _mail.SendAsync(mailData, new CancellationToken());
                            _logger.LogInformation($"Email Status is {EmailStatus}");
                            
                            _logger.LogInformation("Updated {@userInfo}", userInfo);
                             return new ForgetPasswordResponse { Message = Constants.APIErrorMessages.OTP_SENT };

                        }

                        else
                        {
                            _logger.LogError("Email template not found for SignupOtp");
                            return Errors.User.ExceptionMessage;

                        }
                    }
                    return Errors.User.ExceptionMessage;
                }
                /* else
                 {
                     string token = await _userManager.GeneratePasswordResetTokenAsync(user);
                     var identityResult = await _userManager.ResetPasswordAsync(user, token, forgetPasswordRequest.Password);

                     if (identityResult.Succeeded)
                     {
                         _logger.LogInformation("IdentityResult ResetPasswordAsync = {@identityResult}", identityResult);
                         return new ForgetPasswordResponse { Message = Constants.APIErrorMessages.PASSWORD_UPDATED_SUCESSFULLY };
                     }
                     else
                     {
                         _logger.LogError("IdentityResult ResetPasswordAsync = {@identityResult}", identityResult);
                         return Errors.User.ExceptionMessage;
                     }
                 }*/
            }
            catch (Exception ex)
            {

                _logger.LogError("Exception Occur {@ex}", ex);
                return Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<ForgetPasswordResponse>> ForgetPasswordAsync(ForgetPasswordRequest forgetPasswordRequest)
        {

            _logger.LogInformation("ForgetPasswordAsync");
            Domain.Entities.UserInfo userInfo = new Domain.Entities.UserInfo();
            EmailTemplateService emailTemplateService = new EmailTemplateService(_context, _logger);

            MailMapper mailMapper = new MailMapper();
            try
            {
                var otp = int.Parse(forgetPasswordRequest.Otp);
                var isUserInfo = _userInfoRepository.GetOpt(otp);
                if (isUserInfo is null)
                {
                    _logger.LogWarning($"Couldn't found user with this email {forgetPasswordRequest.Otp}");
                    return Errors.User.ErrorInvalidOtpCode;
                }
                 else
                 {
                     string token = await _userManager.GeneratePasswordResetTokenAsync(isUserInfo.User);
                     var identityResult = await _userManager.ResetPasswordAsync(isUserInfo.User, token, forgetPasswordRequest.Password);

                     if (identityResult.Succeeded)
                     {
                         _logger.LogInformation("IdentityResult ResetPasswordAsync = {@identityResult}", identityResult);
                         return new ForgetPasswordResponse { Message = Constants.APIErrorMessages.PASSWORD_UPDATED_SUCESSFULLY };
                     }
                     else
                     {
                         _logger.LogError("IdentityResult ResetPasswordAsync = {@identityResult}", identityResult);
                         return Errors.User.ExceptionMessage;
                     }
                 }
            }
            catch (Exception ex)
            {

                _logger.LogError("Exception Occur {@ex}", ex);
                return Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<PermissionResponse>> GetPermissionsAsync(long userId, string email)
        {
            _logger.LogInformation("GetPermissionsAsync");
            UserInfoMapper userInfoMapper = new UserInfoMapper();
            PermissionResponse permissionResponse = new PermissionResponse();

            try
            {
                List<UserPermission> userPermissionList = await _iuserPermissionRepository.GetUserPermissionByUserIdAsync(userId);
                List<PermissionPermissionType> permissionPermissionTypeList = await _iuserPermissionRepository.GetPermissionPermissionTypeAsync();

                permissionResponse = userInfoMapper.MappingPermissionsDropdownList(userPermissionList, permissionPermissionTypeList);

                _logger.LogInformation("Permission Response {@permissionResponse}", permissionResponse);
                return permissionResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<GenericMessage>> UpdatePermissionsAsync(long userId, string email, UpdatePermission updatePermission)
        {
            _logger.LogInformation("UpdatePermissionsAsync");
            UserInfoMapper userInfoMapper = new UserInfoMapper();
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                using (var dbContextTransaction = _context.Database.BeginTransaction())
                {
                    await _iuserPermissionRepository.DeleteUserPermissionListByUserIdAsync(userId);

                    List<PermissionPermissionType> permissionPermissionTypeList = await _ipermissionPermissionTypeRepository.GetPermissionPermissionTypeListForUpdate(updatePermission);
                    List<UserPermission> userPermissionList = userInfoMapper.MapUserInfoWithUpdatedPermissions(permissionPermissionTypeList, userId, email);

                    userPermissionList = await _iuserPermissionRepository.CreateUserPermissionAsync(userPermissionList);

                    dbContextTransaction.Commit();
                    return new GenericMessage { Message = Constants.APIErrorMessages.PERMISSSIONS_UPDATED };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Errors.User.ExceptionMessage;
            }
        }

        public int createGuId()
        {
            _logger.LogInformation("Generating One-Time Password");
            Random rand = new Random((int)DateTime.Now.Ticks);
            int numIterations = rand.Next(100000, 999999);
            _logger.LogInformation($"One-Time Password is ready {numIterations}");
            return numIterations;
        }




        
        /// <summary>
        /// logout the user which make user device info isInUse false and delete user from waiting list if player
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <param name="logoutModel"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
      

        /// <summary>
        /// Delete the user account soft delete / change the user email
        /// </summary>
        /// <param name="userId">user who is requesting the endpoint</param>
        /// <param name="email">user who is requesting the endpoint</param>
        /// <param name="roleId">user who is requesting the endpoint</param>
        /// <returns></returns>
        public async Task<ErrorOr<GenericMessage>> DeleteUserAccountAsync(long userId, string email, int roleId)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                var userInfo = _userInfoRepository.GetUserInfoByUserId(userId);

                if (userInfo is null || userInfo.User is null)
                {
                    _logger.LogWarning($"Couldn't found user with this email {email}");
                    return Errors.User.UserNotFoundWithThisEmail;
                }
                else
                {
                    var appUser = userInfo.User;

                    var dateTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    var valueEmail = $"{appUser.Email}.DEACTIVATED.{dateTime}";
                    appUser.Email = valueEmail;

                    IdentityResult result = await _userManager.UpdateAsync(appUser);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"{Constants.APIErrorMessages.USER_DELETED}");
                        return new GenericMessage { Message = Constants.APIErrorMessages.USER_DELETED };
                    }

                    _logger.LogWarning($"{Domain.Common.Errors.Errors.User.ErrorUserProfileUpdate}");
                    return Domain.Common.Errors.Errors.User.ErrorUserProfileUpdate;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }


        /// <summary>
        /// check that the provided password matches the current user password
        /// </summary>
        /// <param name="userId">user who is requesting the endpoint</param>
        /// <param name="email">user who is requesting the endpoint</param>
        /// <param name="verifyUserPassword">contains the user password</param>
        /// <returns></returns>
        public async Task<ErrorOr<VerifyUserPasswordResponse>> VerifyUserPasswordAsync(long userId, string email, VerifyUserPassword verifyUserPassword)
        {
            try
            {
                AppUser appUser = await _userManager.FindByEmailAsync(email);

                if (appUser is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.User.ErrorUserNotFound}");
                    return Domain.Common.Errors.Errors.User.ErrorUserNotFound;
                }
                else
                {
                    var response = await _userManager.CheckPasswordAsync(appUser, verifyUserPassword.Password);

                    _logger.LogInformation($"user password is {response}");
                    return new VerifyUserPasswordResponse { IsValid = response };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

    
    }
}
