using ErrorOr;
using FirebaseAdmin.Auth;
using Ensuranx.Application.Contracts;
using Ensuranx.Application.Contracts.EmailTemplate;
using Ensuranx.Application.Contracts.Statkeeper;
using Ensuranx.Application.Contracts.UserInfo;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Requests.Profile;
using Ensuranx.Application.Requests.Statkeeper;
using Ensuranx.Application.Requests.Venue;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Tournament;
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
using Ensuranx.Infrastructure.Services.UserInfo;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
 
using System.Reflection.Emit;
using System.Threading;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Services.Statkeeper
{
    public class StatkeeperService : IStatkeeperService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger _logger;
        private readonly IStatkeeperRepository _istatkeeperRepository;
        private readonly ApplicationDbContext _context;
        private readonly IUserInfoService _iuserInfoService;
        private readonly IEmailService _mail;
        private readonly IEmailTemplateService _iemailTemplateService;
        private readonly IVenueStatkeeperRepository _ivenueStatkeeperRepository;
        private readonly IUserInfoRepository _iuserInfoRepository;
        private readonly IVenueRepository _ivenueRepository;
        private readonly IImagesRepository _imagesRepository;
        private readonly IUserRepository _iuserRepository;
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly IBranchRepository _ibranchRepository;

        public StatkeeperService(ApplicationDbContext context, ILogger logger, UserManager<AppUser> userManager, IEmailService mail)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _context = context;
            _istatkeeperRepository = new StatkeeperRepository(context);
            _userManager = userManager;
            _mail = mail;
            _iuserInfoService = new UserInfoService(context, logger, _userManager, mail);
            _iemailTemplateService = new EmailTemplateService(context, logger);
            _ivenueStatkeeperRepository = new VenueStatkeeperRepository(context);
            _iuserInfoRepository = new UserInfoRepository(context);
            _ivenueRepository = new VenueRepository(context);
            _imagesRepository = new ImagesRepository(context);
            _iuserRepository = new UserRepository(context);
            _ibranchRepository = new BranchRepository(context);
            _userInfoRepository = new UserInfoRepository(context);
        }

        public async Task<ErrorOr<GetStatkeeperById>> GetStatkeeperByIdAsync(long userId)
        {
            _logger.LogInformation("GetStatkeeperByIdAsync");
            VenueStatkeeperMapper venueStatkeeperMapper = new VenueStatkeeperMapper();

            try
            {
                //var user = await _iuserRepository.GetUserByUserId(userId);
                var userInfo = _iuserInfoRepository.GetUserInfoByUserId(userId);
                var venueStatkeeper = await _ivenueRepository.GetVenuesByIdUserIdAsync(userId);
                var branchStatkeeper = await _ivenueRepository.GetBranchesByIdUserIdAsync(userId);
                GetStatkeeperById getStatkeeperById = venueStatkeeperMapper.MapGetStatkeeperById(venueStatkeeper, branchStatkeeper, userInfo);
                _logger.LogInformation("Response {@getStatkeeperById}", getStatkeeperById);
                return getStatkeeperById;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<DeleteStatkeeperById>> DeleteStatkeeperByIdAsync(long userId)
        {
            _logger.LogInformation("DeleteStatkeeperByIdAsync");

            try
            {
                var userInfo = _iuserInfoService.GetUserInfoByUserId(userId);
                if (userInfo is not null)
                {
                    //need to add soft delete functionality globaly
                    userInfo.IsDeleted = true;
                    userInfo.IsActive = false;

                    await _iuserInfoService.UpdateUserInfo(userInfo);
                    _logger.LogInformation("User Updated Successfully {@userInfo}", userInfo);
                    return new DeleteStatkeeperById { Message = Constants.APIErrorMessages.USER_DELETED };
                }
                else
                {
                    _logger.LogInformation("User not found");
                    return Errors.User.ErrorUserNotFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<DeleteStatkeeperById>> DeleteStatkeeperByVenueId(long venueId,long statkeeperId)
        {
            _logger.LogInformation("DeleteStatkeeperByVenueId");

            try
            {
                CancellationToken cancellationToken = CancellationToken.None;
                Ensuranx.Domain.Entities.VenuesStatskeeper venueStatkeeper = await _iuserInfoService.GetVenueStatkeepr(venueId, statkeeperId);
                if (venueStatkeeper is not null)
                {
                    //need to add soft delete functionality globaly
                    venueStatkeeper.IsDeleted = true;
                    venueStatkeeper.IsActive = false;

                    await _iunitOfWork.Repository<VenuesStatskeeper>().UpdateAsync(venueStatkeeper);
                    await _iunitOfWork.Commit(cancellationToken);

                    _logger.LogInformation("User Updated Successfully {@userInfo}", venueStatkeeper);
                    return new DeleteStatkeeperById { Message = Constants.APIErrorMessages.USER_DELETED };
                }
                else
                {
                    _logger.LogInformation("User not found");
                    return Errors.User.ErrorUserNotFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<List<AllStatkeeperResponse>>> GetAllStatkeeperAsync(long venueId, long userId)
        {
            _logger.LogInformation("GetAllStatkeeperAsync");

            try
            {
                var response = await _istatkeeperRepository.GetAllStatkeepersList(venueId);
                List<AllStatkeeperResponse> allStatkeeperResponse = response.Adapt<List<AllStatkeeperResponse>>();
                _logger.LogInformation("all statkeeper response : {@allStatkeeperResponse}", allStatkeeperResponse);

                return allStatkeeperResponse.Any() ? allStatkeeperResponse : Domain.Common.Errors.Errors.User.NoStatkeeperFound;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<List<AllStatkeepersDetails>>> GetAllStatkeeperByUserIdAsync(long userId, BasicFilter paginationFilter)
        {
            _logger.LogInformation("GetAllStatkeeperByUserIdAsync");
            VenueStatkeeperMapper mapper = new VenueStatkeeperMapper();

            try
            {

                var venuestatkeeperList = await _istatkeeperRepository.GetAllStatkeepersByUserIdList(userId, paginationFilter);
                var userinfoList = await _iuserInfoRepository.GetUserInfoByUserId(venuestatkeeperList.Select(x => x.UserInfoId.Value).ToList());
                var venueList = await _ivenueRepository.GetVenuesByIdListAsync(venuestatkeeperList.Select(x => x.VenueId).ToList());
                var imageList = await _imagesRepository.GetImageByUserIdList(userinfoList.Select(x => x.Id).ToList());
                var userList = await _iuserRepository.GetUserByUserIdList(venuestatkeeperList.Select(x => x.UserInfoId.Value).ToList());
                var branchList = await _ibranchRepository.GetBranchByUserId(userId);
                var imageTypeList = await _iunitOfWork.Repository<Domain.Entities.ImageType>().GetAllAsync();

                List<AllStatkeepersDetails> allStatkeepersDetails = mapper.MapAllStatkeepersDetailsResponse(userinfoList, venueList, venuestatkeeperList, imageList, userList, branchList, imageTypeList);

                _logger.LogInformation("all statkeeper response : {@AllStatkeepersDetails}", allStatkeepersDetails);

                return allStatkeepersDetails.Any() ? allStatkeepersDetails : Domain.Common.Errors.Errors.User.NoStatkeeperFound;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<AddStatkeeperResponse>> AddStatkeeperAsync(long userId, string email, AddStatkeeper addStatkeeper)
        {
            _logger.LogInformation("AddStatkeeperAsync");

            UserInfoMapper userInfoMapper = new UserInfoMapper();
            Domain.Entities.UserInfo userInfo = new Domain.Entities.UserInfo();
            MailMapper mailMapper = new MailMapper();
            VenueStatkeeperMapper venueStatkeeperMapper = new VenueStatkeeperMapper();

            try
            {
                if (await _userManager.FindByEmailAsync(addStatkeeper.Email) is not null)
                {
                    _logger.LogWarning("User is already present with this email {email}", addStatkeeper.Email);
                    return Domain.Common.Errors.Errors.User.DuplicateEmail;
                }
                else if (await _userManager.FindByNameAsync(addStatkeeper.Name) is not null)
                {
                    _logger.LogWarning("User is already present with this username {username}", addStatkeeper.Email);
                    return Domain.Common.Errors.Errors.User.DuplicatePhoneNumber;
                }
                else if (await _iuserRepository.CheckPhoneNumber(addStatkeeper.PhoneNumber))
                {
                    _logger.LogWarning("User is already present with this phonenumber {phonenumber}", addStatkeeper.PhoneNumber);
                    return Domain.Common.Errors.Errors.User.DuplicatePhoneNumber;
                }
                else
                {
                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        var user = new AppUser
                        {
                            Email = addStatkeeper.Email,
                            UserName = addStatkeeper.Email,
                            PhoneNumber = addStatkeeper.PhoneNumber,
                        };
                        _logger.LogInformation("Creating New User");

                        var result = await _userManager.CreateAsync(user, addStatkeeper.Password);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("New User {@user} is created", user);

                            _logger.LogInformation("Creating User Role");
                            await _userManager.AddToRoleAsync(user, Roles.Statkeeper.ToString());
                            _logger.LogInformation("User Role Created");

                            int OtpCode = createGuId();

                            userInfo = userInfoMapper.SetDataInUserInfo(user, OtpCode, addStatkeeper.Name, addStatkeeper.ZipCode);
                            _logger.LogInformation("Creating UserInfo");
                            userInfo = await _iuserInfoService.CreateUserInfo(userInfo);
                            _logger.LogInformation("{@userInfo} created", userInfo);

                            var emailTemp = await _iemailTemplateService.GetEmailByTemplateEnum(TemplatesEnum.StatkeeperSignUp);
                            if (emailTemp is not null)
                            {
                                var EmailReplaceBody = emailTemp.Body.Replace("@username", userInfo.Name).Replace("@otpcode", OtpCode.ToString());
                                //Email notification For sign up
                                string EmailSubject = emailTemp.Subject;
                                string EmailBody = EmailReplaceBody;

                                var mailData = mailMapper.SetDataInMailModel(user.Email, EmailSubject, EmailBody, Constants.APIErrorMessages.EMAIL_FROM);
                                _logger.LogInformation($"Send Email to {user.Email} with One-Time Password {OtpCode}");
                                bool EmailStatus = await _mail.SendAsync(mailData, new CancellationToken());
                                _logger.LogInformation($"Email Status is {EmailStatus}");

                                userInfo.OTPCode = OtpCode;
                                userInfo.IsSend = EmailStatus;

                                _logger.LogInformation($"Updating UserInfo");
                                userInfo = await _iuserInfoService.UpdateUserInfo(userInfo);
                                _logger.LogInformation("Updated {@userInfo}", userInfo);

                                _logger.LogInformation("Email Send Successfully");
                                DateTime createdDatetime = DateTime.UtcNow;

                                List<VenuesStatskeeper> venuesStatskeeperList = venueStatkeeperMapper.MapStatkeeperWithVenueList(email, addStatkeeper.VenueIdList, userInfo.Id, createdDatetime);

                                _logger.LogInformation("Creating Venue Statkeepers");
                                venuesStatskeeperList = await _ivenueStatkeeperRepository.AddVenueStatkeeperList(venuesStatskeeperList);
                                _logger.LogInformation("Venue Statkeepers created");

                                dbContextTransaction.Commit();

                                return new AddStatkeeperResponse { Message = Constants.APIErrorMessages.STATKEEPER_CREATED_SUCESSFULLY };
                            }
                            else
                            {
                                return Domain.Common.Errors.Errors.Authentication.EmailTemplateNotFound;
                            }
                        }
                        else
                        {
                            _logger.LogError("User not created sucessfully {@result}", result);
                            return Domain.Common.Errors.Errors.User.UserNotCreatedSuccessfully;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
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
        public async Task<ErrorOr<GenericMessage>> SubstitutePlayer(long teamId, long eventId, long substituteId, long substituteToId, string email, bool isDisqaulify)
        {
            _logger.LogInformation("Substituting Player");
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {
                if (isDisqaulify != true)
                {
                    Application.Response.User.SubstitutePlayer results = new Application.Response.User.SubstitutePlayer();
                    EventMapper eventMapper = new EventMapper();
                    var IsSubstitutePresent = await _istatkeeperRepository.IsMember(teamId, substituteId);
                    var IsSubstituteToPresent = await _istatkeeperRepository.IsMember(teamId, substituteToId);
                    if (IsSubstitutePresent == IsSubstituteToPresent)
                    {
                        Domain.Entities.SubstitutePlayer memberPresent = eventMapper.SubstituteMapper(teamId, eventId, substituteId, substituteToId, email, isDisqaulify);
                        await _iunitOfWork.Repository<Domain.Entities.SubstitutePlayer>().AddAsync(memberPresent);
                        await _iunitOfWork.Commit(cancellationToken);
                        return new GenericMessage { Message = Constants.APIErrorMessages.SUBSTITUTEDSUCCESFULLY };
                    }
                    else
                    {
                        return new GenericMessage { Message = Constants.APIErrorMessages.CANNOTSUBSTITUTE };
                    }
                }
                else
                {
                    EventMapper eventMapper = new EventMapper();
                    Domain.Entities.SubstitutePlayer memberPresent = eventMapper.SubstituteMapper(teamId, eventId, substituteId, substituteToId, email, isDisqaulify);
                    await _iunitOfWork.Repository<Domain.Entities.SubstitutePlayer>().AddAsync(memberPresent);
                    await _iunitOfWork.Commit(cancellationToken);
                    return new GenericMessage { Message = Constants.APIErrorMessages.DISABLE };
                }


            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<List<GenericObj>>> GetVenueByStatkeeperIdAsync(long statkeeperId)
        {
            try
            {
                var venueList = await _istatkeeperRepository.getVenuesByStatkeeperId(statkeeperId);
                return venueList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<GenericMessage>> UpdateStatkeeperAsyn(long userId, string email, UpdateStakeeper updateStakeeper)
        {
            _logger.LogInformation("Update Statkeepr ");
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {
                Domain.Entities.UserInfo userInfo = new Domain.Entities.UserInfo();

                userInfo = _userInfoRepository.GetUserInfoByUserId(updateStakeeper.StatkeeperId);
                var currentUser = userInfo.User;

                if (!string.IsNullOrEmpty(updateStakeeper.Email) && currentUser.Email.ToLower() != updateStakeeper.Email.ToLower())
                    currentUser.Email = updateStakeeper.Email;

                if (!string.IsNullOrEmpty(updateStakeeper.PhoneNumber) && currentUser.PhoneNumber.ToLower() != updateStakeeper.PhoneNumber.ToLower())
                    currentUser.PhoneNumber = updateStakeeper.PhoneNumber;

                if (!string.IsNullOrEmpty(updateStakeeper.FullName))
                    userInfo.Name = updateStakeeper.FullName;

                IdentityResult result = await _userManager.UpdateAsync(currentUser);
               // var statKeepeBio = await _istatkeeperRepository.GetStatkeeperBio(updateStakeeper.StatkeeperId);
               // var userTbl = await _istatkeeperRepository.GetUserTbl(updateStakeeper.StatkeeperId);
               // var user = new AppUser
               // {
               //     Email = updateStakeeper.Email,
               //     UserName = updateStakeeper.FullName,
               //     PhoneNumber = updateStakeeper.PhoneNumber,
               // };
               //var x= await _userManager.SetEmailAsync(user, updateStakeeper.Email);
                 //x = await _userManager.GetUser(user, updateStakeeper.Email);

                //_context.Entry(userTbl).Reference(u => u.User).Load();

                //var userTblEmail = userTbl.User.Email;
                //var userEmail = await _userManager.FindByEmailAsync(userTblEmail);

                //userTbl.Name = updateStakeeper.FullName;
                //userTbl.User.Email = updateStakeeper.Email;
                //userTbl.User.PhoneNumber = updateStakeeper.PhoneNumber;

                //await _iunitOfWork.Repository<Domain.Entities.UserInfo>().UpdateAsync(userTbl);
                //await _iunitOfWork.Commit(cancellationToken);

               
                    string token = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
                    var identityResult = await _userManager.ResetPasswordAsync(currentUser, token, updateStakeeper.Password);

                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation("IdentityResult ResetPasswordAsync = {@identityResult}", identityResult);
                      
                    }

            
                    var venueStatkeeper = _context.VenueStatkeeper.Where(x => x.UserInfoId == updateStakeeper.StatkeeperId).ToList();

                foreach (var item in venueStatkeeper)
                {
                    await _iunitOfWork.Repository<VenuesStatskeeper>().DeleteAsync(item);  
                }
                foreach (var item in updateStakeeper.VenueId)
                {
                    VenuesStatskeeper venuesStatskeeper = new VenuesStatskeeper();

                    venuesStatskeeper.VenueId = item;
                    venuesStatskeeper.CreatedDateTime = DateTime.Now;
                    venuesStatskeeper.LastModifiedDateTime = DateTime.Now;
                    venuesStatskeeper.LastModifiedBy = email;
                    venuesStatskeeper.CreatedBy = email;
                    venuesStatskeeper.UserInfoId = updateStakeeper.StatkeeperId;

                    await _iunitOfWork.Repository<VenuesStatskeeper>().AddAsync(venuesStatskeeper);
                }
                await _iunitOfWork.Commit(cancellationToken);
                return new GenericMessage { Message = Constants.APIErrorMessages.RECORD_UPDATED };
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
    }
}
