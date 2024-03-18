using Azure;
using ErrorOr;
using Ensuranx.Application.Contracts.Venue;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Branch;
using Ensuranx.Application.Requests.Venue;
using Ensuranx.Application.Response.Venue;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Mapster;
using Microsoft.Extensions.Logging;
using Ensuranx.Domain.Entities;
using static Ensuranx.Domain.Enums.Enum;
using Ensuranx.Application.Response.Other;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.Services.Venue
{
    public class VenueService : IVenueService
    {
        private readonly ApplicationDbContext _context;
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;
        private readonly IVenueRepository _ivenueRepository;
        private readonly IVenueStatkeeperRepository _ivenueStatkeeperRepository;
        private readonly IVenueActivityRepository _ivenueActivityRepository;
        private readonly IBranchesVenuesRepository _ibranchesVenuesRepository;
        private readonly IVenueRepository _iVenueRepository;
        private readonly IRoleRepository _iroleRepository;
        private readonly IActivityRepository _iactivityRepository;


        public VenueService(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _ivenueRepository = new VenueRepository(context);
            _ivenueStatkeeperRepository = new VenueStatkeeperRepository(context);
            _ivenueActivityRepository = new VenueActivityRepository(context);
            _iVenueRepository = new VenueRepository(context);
            _iroleRepository = new RoleRepository(context);
            _iactivityRepository = new ActivityRepository(context);
        }

        public async Task<ErrorOr<List<VenueResponse>>> GetAllVenuesAsync(long userId, BranchIdList branchIdList, long roleId)
        {
            try
            {
                var Role = await _iroleRepository.GetRole(userId, roleId);

                branchIdList.branchIdList = branchIdList.branchIdList != null ? branchIdList.branchIdList : branchIdList.branchIdList = new List<long>();

                if (Role == Domain.Enums.Enum.Roles.Statkeeper.ToString())
                {
                    var result = await _ivenueRepository.GetAllVenuesAsyncByStatkeeper(userId, branchIdList.branchIdList);
                    List<VenueResponse> responses = result.Adapt<List<VenueResponse>>();
                    //var TotalRecords = await _ivenueRepository.GetAllVenuesCountByStatKeeper(userId, branchIdList.branchIdList);
                    return responses;
                    // return new List<VenueResponse>(TotalRecords, responses,0,0);
                }
                else if (Role == Domain.Enums.Enum.Roles.Business.ToString())
                {
                    var result = await _ivenueRepository.GetAllVenuesAsyncByBusiness(userId, branchIdList.branchIdList, branchIdList.statkeeperId, roleId);
                    List<VenueResponse> responses = result.Adapt<List<VenueResponse>>();
                    //var TotalRecords = await _ivenueRepository.GetAllVenuesCountByBusiness(userId, branchIdList.branchIdList);
                    return responses;

                    //return new List<VenueResponse>(TotalRecords, responses);
                }
                else
                {
                    return Domain.Common.Errors.Errors.User.ExceptionMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }


        public async Task<Domain.Entities.Venue> CreateVenueAsync(Domain.Entities.Venue venue)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            venue = await _iunitOfWork.Repository<Domain.Entities.Venue>().AddAsync(venue);
            await _iunitOfWork.Commit(cancellationToken);
            return venue;
        }

        public async Task<ErrorOr<AddVenueResponse>> AddVenueAsync(long userId, string email, AddVenue addVenue)
        {
            _logger.LogInformation("GetBranchesDetailsAsync");
            VenueStatkeeperMapper venueStatkeeperMapper = new VenueStatkeeperMapper();
            VenueActivityMapper venueActivityMapper = new VenueActivityMapper();

            try
            {
                DateTime createdDatetime = DateTime.UtcNow;
                TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

                var config = TypeAdapterConfig.GlobalSettings;

                TypeAdapterConfig<(AddVenue addVenue, string email, DateTime createdDatetime), Domain.Entities.Venue>.NewConfig()
                    .Map(dest => dest.CreatedBy, src => src.email)
                    .Map(dest => dest.LastModifiedBy, src => src.email)
                    .Map(dest => dest.LastModifiedDateTime, src => src.createdDatetime)
                    .Map(dest => dest.CreatedDateTime, src => src.createdDatetime)
                    .Map(dest => dest, src => src.addVenue);
                Domain.Entities.Venue venue = (addVenue, email, createdDatetime).Adapt<Domain.Entities.Venue>();

                using (var dbContextTransaction = _context.Database.BeginTransaction())
                {
                    _logger.LogInformation("Creating Venue {@venue}", venue);
                    venue = await CreateVenueAsync(venue);
                    _logger.LogInformation("Venue created {@venue}", venue);

                    if (addVenue.StatkeeperIdList.Any())
                    {
                        List<VenuesStatskeeper> venuesStatskeeperList = venueStatkeeperMapper.MapVenueWithStatkeeperList(email, venue.Id, addVenue.StatkeeperIdList, createdDatetime);

                        _logger.LogInformation("Creating Venue Statkeepers");
                        venuesStatskeeperList = await _ivenueStatkeeperRepository.AddVenueStatkeeperList(venuesStatskeeperList);
                        _logger.LogInformation("Venue Statkeepers created");
                    }

                    if (addVenue.ActivityIdList.Any())
                    {
                        List<Domain.Entities.Activity> activityList = await _iactivityRepository.GetActivityListByEventTypeAndActivityCategoryId(activityCategoryIdList: addVenue.ActivityIdList);
                        List<long> activityIdList = activityList.Select(x => x.Id).ToList();
                        List<VenueActivity> venueActivityList = venueActivityMapper.MapVenueWithStatkeeperList(email, venue.Id, activityIdList, createdDatetime);

                        _logger.LogInformation("Creating Venue activities");
                        venueActivityList = await _ivenueActivityRepository.AddVenueActivityList(venueActivityList);
                        _logger.LogInformation("Venue activities created");
                    }

                    dbContextTransaction.Commit();
                    return new AddVenueResponse { Message = Constants.APIErrorMessages.VENUE_CREATED_SUCESSFULLY };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<PagedResponse<List<VenueEventListResponse>>>> GetAllVenueEventList(BasicFilter paginationFilter, long userId, long roleId)
        {
            try
            {
                var role = await _iroleRepository.GetRole(userId, roleId);
                if (role == Domain.Enums.Enum.Roles.Player.ToString())
                {
                    VenueEventMapper mapper = new VenueEventMapper();
                    var result = await _iVenueRepository.GetVenuesEventWithoutTeamsPlayer(paginationFilter, userId);
                    List<long> eventIdList = result.Select(x => x.Id).ToList();
                    var teamsPlayer = await _iVenueRepository.GetAllPlayersByPlayer(eventIdList);
                    //var teams = await _iVenueRepository.GetAllTeamsByVenueId(result.Select(x => x.Id).Distinct().ToList());

                    List<VenueEventListResponse> VenueEventList = mapper.MapVenueEventResponse(result, teamsPlayer);
                    // var TotalRecords = VenueEventList.Count();
                    var TotalRecords = await _ivenueRepository.GetVenuesEventWithoutTeamsPlayerCount(userId, paginationFilter.sSearch);

                    return new PagedResponse<List<VenueEventListResponse>>(TotalRecords, VenueEventList, paginationFilter.PageNumber, paginationFilter.PageSize);
                }
                else if (role == Domain.Enums.Enum.Roles.Business.ToString())
                {
                    VenueEventMapper mapper = new VenueEventMapper();
                    var result = await _iVenueRepository.GetVenuesEventWithoutTeamsBusiness(paginationFilter, userId);
                    List<long> eventIdList = result.Select(x => x.Id).ToList();
                    var teamsPlayer = await _iVenueRepository.GetAllPlayers(eventIdList);
                    //var teams = await _iVenueRepository.GetAllTeamsByVenueId(result.Select(x => x.Id).Distinct().ToList());

                    List<VenueEventListResponse> VenueEventList = mapper.MapVenueEventResponse(result, teamsPlayer);

                    var TotalRecords = await _ivenueRepository.GetVenuesEventWithoutTeamsBusinessCount(paginationFilter, userId);

                    return new PagedResponse<List<VenueEventListResponse>>(TotalRecords, VenueEventList, paginationFilter.PageNumber, paginationFilter.PageSize);
                }
                else if (role == Domain.Enums.Enum.Roles.Statkeeper.ToString())
                {
                    VenueEventMapper mapper = new VenueEventMapper();
                    //var EventCollection = await _iunitOfWork.Repository<Domain.Entities.EventCollection>().GetAllAsync();

                    //  List<StatkeeperInVenue> StatkeeperInVenue = await _iVenueRepository.StatkeeperInVenue(paginationFilter, userId);

                    // Converting results into VenueEventListResponse for testing purpose;
                    var result = await _iVenueRepository.GetVenuesEventWithoutTeamsStatkeeper(paginationFilter, userId);
                    List<long> eventIdList = result.Select(x => x.Id).ToList();

                    var teamsPlayer = await _iVenueRepository.GetAllPlayers(eventIdList);


                    List<VenueEventListResponse> VenueEventList = mapper.MapVenueEventResponse(result, teamsPlayer);
                    //var TotalRecords = VenueEventList.Count();
                    var TotalRecords = await _iVenueRepository.GetVenuesEventWithoutTeamsStatkeeperCount(userId, paginationFilter.sSearch);
                    return new PagedResponse<List<VenueEventListResponse>>(TotalRecords, VenueEventList, paginationFilter.PageNumber, paginationFilter.PageSize);
                }
                else
                {
                    return Domain.Common.Errors.Errors.User.ExceptionMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        /// <summary>
        /// VenueEventListUpdateReq Contains venueId againt which we are updating eventCollection
        /// 
        /// </summary>
        /// <param name="VenueEventListUpdateReq"></param>
        /// <returns></returns>
        public async Task<ErrorOr<string>> UpdateEventVenueList(UpdateEventVenueListReq VenueEventListUpdateReq)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {
                _logger.LogInformation("UpdateBranchesAsync");
                VenueEventMapper mapper = new VenueEventMapper();

                // var EventCollection = await _iunitOfWork.Repository<Domain.Entities.EventCollection>().GetByIdAsync(VenueEventListUpdateReq.Id);
                var Event = await _iunitOfWork.Repository<Domain.Entities.Event>().GetByIdAsync(VenueEventListUpdateReq.Id);
                var EventStatusId = await _iVenueRepository.EventStatusId(Event);
                var EventTeamId = await _iVenueRepository.EventTeamId(Event);
                if (EventStatusId.ToString() != Domain.Enums.Enum.EventStatus.Active.ToString().ToLower())
                {

                    var EventCollection = await _iunitOfWork.Repository<Domain.Entities.EventCollection>().GetByIdAsync(EventTeamId);
                    EventCollection = mapper.MapEventCollection(EventCollection, VenueEventListUpdateReq);

                    await _iunitOfWork.Repository<Domain.Entities.EventCollection>().UpdateAsync(EventCollection);
                    await _iunitOfWork.Commit(cancellationToken);
                    _logger.LogInformation("Event Collection updated succesfully ", EventCollection);

                    return Constants.APIErrorMessages.RECORD_UPDATED;
                }
                else
                {
                    return Domain.Common.Errors.Errors.User.ExceptionMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }

        }


        public async Task<ErrorOr<string>> UpdateVenueStatkeeper(UpdateVenueStatkeeperReq UpdateVenueStatkeeperReq)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            var eventStatus = await _iunitOfWork.Repository<Domain.Entities.EventStatus>().GetByIdAsync(UpdateVenueStatkeeperReq.Id);

            try
            {
                _logger.LogInformation("UpdateBranchesAsync");
                VenueEventMapper mapper = new VenueEventMapper();


                var EventCollection = await _iunitOfWork.Repository<Domain.Entities.EventCollection>().GetByIdAsync(UpdateVenueStatkeeperReq.Id);
                EventCollection = mapper.MapBranchVenue(EventCollection, UpdateVenueStatkeeperReq);

                await _iunitOfWork.Repository<Domain.Entities.EventCollection>().UpdateAsync(EventCollection);
                await _iunitOfWork.Commit(cancellationToken);
                _logger.LogInformation("Event Collection updated succesfully ", EventCollection);

                return Constants.APIErrorMessages.RECORD_UPDATED;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<BranchesVenuesListById>> BranchesVenuesListById(long EventId)
        {
            _logger.LogInformation("BranchesVenuesListById");

            BranchesVenuesListById BranchesVenuesListById = new BranchesVenuesListById();
            try
            {
                var results = await _ivenueRepository.GetBranchesVenuesListById(EventId);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<DeleteEventCollection>> DeleteEventCollectionById(long eventCollectionId)
        {
            _logger.LogInformation("DeleteEventCollectionById");
            DeleteEventCollection Results = new DeleteEventCollection();

            try
            {
                var EventCollection = await _iunitOfWork.Repository<Domain.Entities.EventCollection>().GetByIdAsync(eventCollectionId);
                if (EventCollection.IsDeleted != true && EventCollection.IsActive != false)
                {
                    CancellationToken cancellationToken = CancellationToken.None;

                    EventCollection.IsActive = false;
                    EventCollection.IsDeleted = true;

                    await _iunitOfWork.Repository<Ensuranx.Domain.Entities.EventCollection>().UpdateAsync(EventCollection);
                    await _iunitOfWork.Commit(cancellationToken);
                    //return userInfo;
                    _logger.LogInformation("EventCollection Updated Successfully {@EventCollection}", EventCollection);
                    return new DeleteEventCollection { Message = Constants.APIErrorMessages.EVENTCOLLECTION_DELETED };
                }
                else
                {
                    return Domain.Common.Errors.Errors.User.RecordNotFound;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<List<VenueByBrandIdResp>>> GetVenuesByBranchId(long branchId)
        {
            try
            {
                VenueEventMapper mapper = new VenueEventMapper();
                var getVenuesDetails = await _iVenueRepository.GetVenuesDetail(branchId);
                var venuesId = getVenuesDetails.Select(x => x.Id).Distinct().ToList();
                var currentActivty = await _iVenueRepository.GetCurrentActivity(venuesId);
                var eventId = currentActivty.Select(x => x.eventId).Distinct().ToList();
                var getStatkeeperDetails = await _ivenueRepository.GetStatkeeperDetails(venuesId);
                var teamsPlayer = await _iVenueRepository.GetAllPlayersByPlayer(eventId);
                var results = mapper.GetAllVenuesRespMapper(getVenuesDetails, currentActivty, getStatkeeperDetails, teamsPlayer);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }


        public async Task<ErrorOr<GenericMessage>> UpdateVenue(UpdateVenueReq UpdateVenueReq, string email)
        {
            try
            {
                CancellationToken cancellationToken = CancellationToken.None;

                var Venue = await _iunitOfWork.Repository<Domain.Entities.Venue>().GetByIdAsync(UpdateVenueReq.VenueId);
                Domain.Entities.Venue updateRecord = new Domain.Entities.Venue();

                Venue.Name = UpdateVenueReq.Name;
                Venue.LastModifiedDateTime = DateTime.Now;
                Venue.LastModifiedBy = email;
                    
                await _iunitOfWork.Repository<Domain.Entities.Venue>().UpdateAsync(Venue);
                await _iunitOfWork.Commit(cancellationToken);

                var venueStatkeeper = _context.VenueStatkeeper.Where(x => x.VenueId == UpdateVenueReq.VenueId).ToList();
                foreach (var item in venueStatkeeper)
                {
                    await _iunitOfWork.Repository<VenuesStatskeeper>().DeleteAsync(item);
                    await _iunitOfWork.Commit(cancellationToken);
                }
                foreach (var item in UpdateVenueReq.StatkeeperIdList)
                {
                    VenuesStatskeeper venuesStatskeeper = new VenuesStatskeeper();

                    venuesStatskeeper.VenueId = UpdateVenueReq.VenueId;
                    venuesStatskeeper.CreatedDateTime = DateTime.Now;
                    venuesStatskeeper.LastModifiedDateTime = DateTime.Now;
                    venuesStatskeeper.LastModifiedBy = email;
                    venuesStatskeeper.CreatedBy = email;
                    venuesStatskeeper.UserInfoId = item;

                    await _iunitOfWork.Repository<VenuesStatskeeper>().AddAsync(venuesStatskeeper);
                    await _iunitOfWork.Commit(cancellationToken);
                }
                var venueActivity = _context.VenueActivity.Where(x => x.VenueId == UpdateVenueReq.VenueId).ToList();
                foreach (var item in venueActivity)
                {
                    await _iunitOfWork.Repository<VenueActivity>().DeleteAsync(item);
                    await _iunitOfWork.Commit(cancellationToken);
                }
                var activity = await _context.Activity
                                     .Where(x => UpdateVenueReq.ActivityIdList.Contains(x.ActivityCategoryId))
                                     .Select(x => x.Id).ToListAsync();
                foreach (var item in activity)
                {
                    VenueActivity venueActivityTemp = new VenueActivity();

                    venueActivityTemp.VenueId = UpdateVenueReq.VenueId;
                    venueActivityTemp.CreatedDateTime = DateTime.Now;
                    venueActivityTemp.LastModifiedDateTime = DateTime.Now;
                    venueActivityTemp.LastModifiedBy = email;
                    venueActivityTemp.CreatedBy = email;
                    venueActivityTemp.ActivityId = item;
                    await _iunitOfWork.Repository<VenueActivity>().AddAsync(venueActivityTemp);
                    await _iunitOfWork.Commit(cancellationToken);

                }
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
