using ErrorOr;
using FirebaseAdmin.Auth;
using Ensuranx.Application.Contracts.Branches;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Branch;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Team;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Common.Wrapper;
using Ensuranx.Common.WrapperInterface;
using Ensuranx.Domain.Common.Errors;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Services.Branches
{
    /// <summary>
    /// all the actions related to branch entity
    /// </summary>
    public class BranchesService : IBranchesService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;
        private readonly IBranchesVenuesRepository _ibranchesVenuesRepository;
        private readonly IBranchRepository _ibranchRepository;
        private readonly IConnectionEntityRepository _iconnectionEntityRepository;
        private readonly IConnectionTypeRepository _iconnectionTypeRepository;
        private readonly IConnectionRepository _iconnectionRepository;
        private readonly IBranchSchedulingRepository _ibranchSchedulingRepository;
        private readonly ITeamRepository _iteamRepository;
        private readonly IRoleRepository _iroleRepository;
        private readonly ApplicationDbContext _context;
        public BranchesService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _context = context;
            _ibranchesVenuesRepository = new BranchesVenuesRepository(context);
            _ibranchRepository = new BranchRepository(context);
            _iconnectionEntityRepository = new ConnectionEntityRepository(context);
            _iconnectionTypeRepository = new ConnectionTypeRepository(context);
            _iconnectionRepository = new ConnectionRepository(context);
            _ibranchSchedulingRepository = new BranchSchedulingRepository(context);
            _iroleRepository = new RoleRepository(context);
            _iteamRepository = new TeamRepository(context);
        }

        /// <summary>
        /// results and teams are getting respective model then both send to mapper, from mapper we are getting our response.
        /// </summary>
        /// <param name="paginationFilter"></param>
        /// <param name="userId"></param>
        /// <returns></returns>

        public async Task<ErrorOr<PagedResponse<List<BranchesVenuesResponse>>>> GetAllBranchesVenuesAsync(BasicFilter paginationFilter, long userId,long roleId,long teamId, long tournamentId,
            long venueId, Domain.Enums.Enum.EventStatus eventStatus, long activityCategoryId)
        {
            _logger.LogInformation("GetMatches");
            try
            {   
                var Role = await _iroleRepository.GetRole(userId, roleId);

                EventMapper mapper = new EventMapper();
                if (Role == Domain.Enums.Enum.Roles.Business.ToString())
                {
                   // var temp = await _ibranchesVenuesRepository.GetBranchesVenuesListByPlayerTemp(paginationFilter, userId, teamId, tournamentId, venueId, eventStatus, activityCategoryId);

                    var result = await _ibranchesVenuesRepository.GetBranchesVenuesListByBusiness(paginationFilter, userId, teamId, tournamentId, venueId, eventStatus);
                    var Teams = await _ibranchesVenuesRepository.GetBranchesVenuesListByTeam(result.Select(x => x.Id).Distinct().ToList());
                    var ActivityIcon = await _ibranchesVenuesRepository.GetBranchesVenuesListActivityIcons(result.Select(x => x.activityId).Distinct().ToList());
                    var Statkeeper = await _ibranchesVenuesRepository.GetBranchesVenuesListStatkeeperIcons(result.Select(x => x.StatkeeperId).Distinct().ToList());
                    List<BranchesVenuesResponse> matchesByBusiness = mapper.StatusMapper(paginationFilter, result, Teams, ActivityIcon, Statkeeper, Role);
                    var TotalRec = mapper.StatusMapperCount(paginationFilter, result, Teams, ActivityIcon, Statkeeper, Role);
                    var TotalRecords = await _ibranchesVenuesRepository.GetBranchesVenuesCount(userId, teamId, tournamentId,venueId, eventStatus,paginationFilter.sSearch);
                  //  var TotalRecords = result.Count();
                    return new PagedResponse<List<BranchesVenuesResponse>>(TotalRec, matchesByBusiness, paginationFilter.PageNumber, paginationFilter.PageSize);
                }
                else if (Role == Domain.Enums.Enum.Roles.Statkeeper.ToString())
                {
                    var result = await _ibranchesVenuesRepository.GetBranchesVenuesListByStatkeeper(paginationFilter, userId, teamId, tournamentId, venueId, eventStatus);
                    var Teams = await _ibranchesVenuesRepository.GetBranchesVenuesListByTeam(result.Select(x => x.Id).Distinct().ToList());
                    var ActivityIcon = await _ibranchesVenuesRepository.GetBranchesVenuesListActivityIcons(result.Select(x => x.activityId).Distinct().ToList());
                    var Statkeeper = await _ibranchesVenuesRepository.GetBranchesVenuesListStatkeeperIcons(result.Select(x => x.StatkeeperId).Distinct().ToList());
                    List<BranchesVenuesResponse> matchesByStatkeepers = mapper.StatusMapper(paginationFilter,result, Teams, ActivityIcon, Statkeeper, Role);
                    int TotalRecords = await _ibranchesVenuesRepository.GetBranchesVenuesListByStatkeeperCount(userId,tournamentId, teamId, venueId, eventStatus,paginationFilter.sSearch);
                    return new PagedResponse<List<BranchesVenuesResponse>>(TotalRecords, matchesByStatkeepers, paginationFilter.PageNumber, paginationFilter.PageSize);
                }
                else if (Role == Domain.Enums.Enum.Roles.Player.ToString())
                {
                    var result = await _ibranchesVenuesRepository.GetBranchesVenuesListByPlayer(paginationFilter, userId, teamId, tournamentId, venueId, eventStatus, activityCategoryId);
                    var TotalRecords = await _ibranchesVenuesRepository.GetBranchesVenuesPlayerCount(paginationFilter, userId, tournamentId, teamId, venueId, eventStatus, activityCategoryId,paginationFilter.sSearch);

                    return new PagedResponse<List<BranchesVenuesResponse>>(TotalRecords, result, paginationFilter.PageNumber, paginationFilter.PageSize);
                }

                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// update branch details in profile tab
        /// </summary>
        /// <param name="updateBranch">model contains thew updated branch details</param>
        /// <param name="email">user email who is updating</param>
        /// <returns>string success message</returns>
        public async Task<ErrorOr<UpdateBranchResponse>> UpdateBranchesAsync(UpdateBranch updateBranch, string email)
        {
            _logger.LogInformation("UpdateBranchesAsync");
            ConnectionMapper connectionMapper = new ConnectionMapper();
            ImageMapper imageMapper = new ImageMapper();
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                var branch = await _iunitOfWork.Repository<Branch>().GetByIdAsync(updateBranch.Id);
                if (branch is null || updateBranch.Id == 0)
                {
                    return Errors.User.BranchRequired;
                }
                else
                {
                    var connectionEntity = await _iconnectionEntityRepository.GetConnectionEntityAsync(Domain.Enums.Enum.ConnectionEntity.Branch.ToString());
                    _logger.LogInformation("Branch fetched");
                    var connectionList = await _iconnectionRepository.GetConnectionListByConnectionEntityAndTableId(branch.Id, connectionEntity.Id);
                    connectionList = connectionMapper.MapConnectionListForUpdateBranch(connectionList, updateBranch);
                    var imageTypeList = await _iunitOfWork.Repository<Domain.Entities.ImageType>().GetAllAsync();
                    var connectionEntityList = await _iunitOfWork.Repository<Domain.Entities.ConnectionEntity>().GetAllAsync();

                    long profileImageType = imageTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ImageType.Profile.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();
                    long coverImageType = imageTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ImageType.Cover.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                    long branchConnectionEntityId = connectionEntityList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.ConnectionEntity.Branch.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        connectionList = await _iconnectionRepository.UpdateConnectionList(connectionList);
                        _logger.LogInformation("Branch Connection Updated Sucessfully");

                        if (!string.IsNullOrEmpty(updateBranch.Name))
                        {
                            branch.Name = updateBranch.Name;
                            await _iunitOfWork.Repository<Branch>().UpdateAsync(branch);
                            await _iunitOfWork.Commit(cancellationToken);
                            _logger.LogInformation("branch updated sucessfully {@branch}", branch);
                        }
                        if (updateBranch.BranchSchedulingDetailList.Any())
                        {
                            var getBranchScheduleByBranchId = _context.BranchScheduling.Where(x => x.BranchId == updateBranch.Id).ToList();
                            foreach (var BranchScheduleByBranchId in getBranchScheduleByBranchId)
                            {
                                await _iunitOfWork.Repository<BranchScheduling>().DeleteAsync(BranchScheduleByBranchId);
                                await _iunitOfWork.Commit(cancellationToken);
                            }
                            foreach (var branchSchedule in updateBranch.BranchSchedulingDetailList)
                            {
                               // var getBranchSchedules = await _iunitOfWork.Repository<BranchScheduling>().GetByIdAsync(updateBranch.Id);

                                BranchScheduling getBranchSchedule = new BranchScheduling();

                                getBranchSchedule.StartTime = TimeSpan.Parse(branchSchedule.StartTime);
                                getBranchSchedule.EndTime = TimeSpan.Parse(branchSchedule.EndTime);
                                getBranchSchedule.WeekDay = branchSchedule.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Monday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Monday :
                                                            branchSchedule.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Tuesday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Tuesday :
                                                            branchSchedule.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Wednesday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Wednesday :
                                                            branchSchedule.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Thursday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Thursday :
                                                            branchSchedule.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Friday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Friday :
                                                            branchSchedule.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Saturday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Saturday :
                                                            branchSchedule.WeekDays.ToLower().Trim() == Domain.Enums.Enum.WeekDays.Sunday.ToString().ToLower().Trim() ? Domain.Enums.Enum.WeekDays.Sunday :
                                                            Domain.Enums.Enum.WeekDays.Monday;
                                getBranchSchedule.CreatedBy = email;
                                getBranchSchedule.LastModifiedBy = email;
                                getBranchSchedule.BranchId = updateBranch.Id;
                                getBranchSchedule.CreatedDateTime = DateTime.UtcNow;
                                getBranchSchedule.LastModifiedDateTime = DateTime.UtcNow;
                                getBranchSchedule.Adapt<UpdateBranch>();
                                // getBranchSchedule.WeekDay = branchSchedule.WeekDays;

                                await _iunitOfWork.Repository<BranchScheduling>().AddAsync(getBranchSchedule);
                                await _iunitOfWork.Commit(cancellationToken);
                            }
                            _logger.LogInformation("branch scheduling updated sucessfully");
                        }

                        if (!string.IsNullOrEmpty(updateBranch.Profile) && updateBranch.ProfileId == 0)
                        {
                            _logger.LogInformation("Creating profile image");
                            var createImage = imageMapper.MapImageForCreate(updateBranch.Id, updateBranch.Profile, branchConnectionEntityId, profileImageType, email);
                            await _iunitOfWork.Repository<Domain.Entities.Images>().AddAsync(createImage);
                            await _iunitOfWork.Commit(cancellationToken);
                            _logger.LogInformation("profile image created {@createImage}", createImage);
                        }
                        else if (!string.IsNullOrEmpty(updateBranch.Profile) && updateBranch.ProfileId != 0)
                        {
                            _logger.LogInformation("Updating profile image");
                            var profileImage = await _iunitOfWork.Repository<Domain.Entities.Images>().GetByIdAsync(updateBranch.ProfileId);
                            profileImage.Path = updateBranch.Profile;
                            await _iunitOfWork.Repository<Domain.Entities.Images>().UpdateAsync(profileImage);
                            await _iunitOfWork.Commit(cancellationToken);
                            _logger.LogInformation("profile image updated {@profileImage}", profileImage);
                        }

                        if (!string.IsNullOrEmpty(updateBranch.Cover) && updateBranch.CoverId == 0)
                        {
                            _logger.LogInformation("Creating cover image");
                            var createImage = imageMapper.MapImageForCreate(updateBranch.Id, updateBranch.Cover, branchConnectionEntityId, coverImageType, email);
                            await _iunitOfWork.Repository<Domain.Entities.Images>().AddAsync(createImage);
                            await _iunitOfWork.Commit(cancellationToken);
                            _logger.LogInformation("cover image created {@createImage}", createImage);
                        }
                        else if (!string.IsNullOrEmpty(updateBranch.Cover) && updateBranch.CoverId != 0)
                        {
                            _logger.LogInformation("Updating cover image");
                            var coverImage = await _iunitOfWork.Repository<Domain.Entities.Images>().GetByIdAsync(updateBranch.CoverId);
                            coverImage.Path = updateBranch.Cover;
                            await _iunitOfWork.Repository<Domain.Entities.Images>().UpdateAsync(coverImage);
                            await _iunitOfWork.Commit(cancellationToken);
                            _logger.LogInformation("profile image updated {@coverImage}", coverImage);
                        }

                        dbContextTransaction.Commit();
                        return new UpdateBranchResponse { Message = Constants.APIErrorMessages.BRANCH_UPDATED };
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// get all branches id and names
        /// </summary>
        /// <param name="userId">the user who is requesting (branches under this user)</param>
        /// <returns>model contain id and name for branch</returns>
        public async Task<ErrorOr<List<AllBranchesResponse>>> GetAllBranchesAsync(long userId, long teamId)
        {
            _logger.LogInformation("GetBranchesDetailsAsync");

            try
            {
                // for Branche Images


                BranchesDetailsMapper mapper = new BranchesDetailsMapper();

                var response = await _ibranchRepository.GetAllBranchesList(userId);
                var BranchIdName = response.Select(x => new { x.Id, x.Name }).ToList();
                var branchIds = BranchIdName.Select(x => x.Id).ToList();
                var statkeeperDetailList = await _ibranchRepository.GetStatkeeeperDetails(branchIds);

                var results = mapper.GetAllBranchesRespMapper(response, statkeeperDetailList);
                //List<AllBranchesResponse> allBranchesResponse = response.Adapt<List<AllBranchesResponse>>();
                _logger.LogInformation("all branched response : {@allBranchesResponse}", results);

                return results.Any() ? results : Domain.Common.Errors.Errors.User.UserHaveNoBranches;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }


        public async Task<ErrorOr<List<GenericObj>>> GetAllBranchesAsync(long userId)
        {
            try
            {
                var results = await _ibranchRepository.GetAllBranchesListByID(userId);

                _logger.LogInformation("all branched response : {@allBranchesResponse}", results);

                return results.Any() ? results : Domain.Common.Errors.Errors.User.UserHaveNoBranches;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        /// <summary>
        /// create new branch
        /// </summary>
        /// <param name="userId">user who is creating new branch</param>
        /// <param name="email">user email</param>
        /// <param name="addBranch">branch details name,address,phone number,website etc</param>
        /// <returns>returns success message</returns>
        public async Task<ErrorOr<AddBranchResponse>> AddBranchAsync(long userId, string email, AddBranch addBranch)
        {
            _logger.LogInformation("AddBranchesAsync");
            ConnectionMapper connectionMapper = new ConnectionMapper();
            BranchSchedulingMapper branchSchedulingMapper = new BranchSchedulingMapper();
            List<Domain.Entities.ConnectionType> connectionTypeList = new List<Domain.Entities.ConnectionType>();
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                var connectionEntity = await _iconnectionEntityRepository.GetConnectionEntityAsync(Domain.Enums.Enum.ConnectionEntity.Branch.ToString());

                var connectionTypeAddress = await _iconnectionTypeRepository.GetConnectionTypeAsync(Domain.Enums.Enum.ConnectionType.Address.ToString());
                connectionTypeList.Add(connectionTypeAddress);
                var connectionTypeEmail = await _iconnectionTypeRepository.GetConnectionTypeAsync(Domain.Enums.Enum.ConnectionType.Email.ToString());
                connectionTypeList.Add(connectionTypeEmail);
                var connectionTypeFacebook = await _iconnectionTypeRepository.GetConnectionTypeAsync(Domain.Enums.Enum.ConnectionType.Facebook.ToString());
                connectionTypeList.Add(connectionTypeFacebook);
                var connectionTypePhoneNumber = await _iconnectionTypeRepository.GetConnectionTypeAsync(Domain.Enums.Enum.ConnectionType.PhoneNumber.ToString());
                connectionTypeList.Add(connectionTypePhoneNumber);
                var connectionTypeTwitter = await _iconnectionTypeRepository.GetConnectionTypeAsync(Domain.Enums.Enum.ConnectionType.Twitter.ToString());
                connectionTypeList.Add(connectionTypeTwitter);
                var connectionTypeWebsite = await _iconnectionTypeRepository.GetConnectionTypeAsync(Domain.Enums.Enum.ConnectionType.Website.ToString());
                connectionTypeList.Add(connectionTypeWebsite);
                
                Branch branch = addBranch.Adapt<Branch>();

                branch.UserInfoId = userId;
                branch.CreatedBy = email;
                branch.LastModifiedBy = email;
                branch.CreatedDateTime = DateTime.UtcNow;
                branch.LastModifiedDateTime = DateTime.UtcNow;

                using (var dbContextTransaction = _context.Database.BeginTransaction())
                {
                    branch = await _iunitOfWork.Repository<Branch>().AddAsync(branch);
                    await _iunitOfWork.Commit(cancellationToken);

                    List<Connection> connectionList = connectionMapper.MapConnectionForBranch(branch, addBranch, connectionEntity, connectionTypeList, email);
                    connectionList = await _iconnectionRepository.CreateConnectionList(connectionList);

                    List<BranchScheduling> branchSchedulingList = branchSchedulingMapper.MapBranchScheduling(email, addBranch, branch);

                    branchSchedulingList = await _ibranchSchedulingRepository.CreateBranchScheduling(branchSchedulingList);
                    dbContextTransaction.Commit();

                    return new AddBranchResponse { Message = Constants.APIErrorMessages.BRANCH_CREATED };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }


        public async Task<ErrorOr<List<MatchesByIdRes>>> GetMatchesById(long eventCollectionId,long userId)
        {
            _logger.LogInformation("GetMatchesById");
            try
            {
                _logger.LogInformation("GetMatchesById");
                var response = await _ibranchRepository.GetMatchesByIdRepo(eventCollectionId,userId);
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<DeleteStatkeeperById>> DeleteBranchByIdAsync(long BranchId)
        {
            _logger.LogInformation("DeleteBranchByIdAsync");

            try
            {
                var getBranch = _context.Branch.FirstOrDefault(x => x.Id == BranchId);
                if (getBranch is not null)
                {
                    //need to add soft delete functionality globaly
                    getBranch.IsDeleted = true;
                    getBranch.IsActive = false;

                    await _ibranchRepository.UpdateBranch(getBranch);
                    _logger.LogInformation("Branch Deleted Successfully {@userInfo}", getBranch);
                    return new DeleteStatkeeperById { Message = Constants.APIErrorMessages.BRANCH_DELETED };
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
        public async Task<ErrorOr<BranchDetailByBranchId>> GetBranchDetailByBranchIdAsyn(long branchId)
        {
            try
            {
                var branchDetail = await _ibranchRepository.getBranchDetail(branchId);
                var branchSchdule = await _ibranchRepository.GetBranchSchedulingListById(branchId);
                var branchConnections = await _ibranchRepository.connectionDetailsbyBranchIds(branchId);

                branchDetail.branchSchedulings = branchSchdule;
                branchDetail.connections=branchConnections;
                return branchDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
    }
}
