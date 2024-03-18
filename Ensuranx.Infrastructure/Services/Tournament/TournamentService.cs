using ErrorOr;
using FirebaseAdmin.Auth;
using Ensuranx.Application.Contracts.Tournament;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Statkeeper;
using Ensuranx.Application.Requests.Team;
using Ensuranx.Application.Requests.Tournament;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Team;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
 
using System.Collections.Generic;
using System.Text;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Services.Tournament
{
    public class TournamentService : ITournamentService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly ITournamentRepository _itournamentRepository;
        private readonly ITournamentTeamRepository _itournamentTeamRepository;
        private readonly ITournamentVenueRepository _itournamentVenueRepository;
        private readonly IEventCollectionTeamRepository _ieventCollectionTeamRepository;
        private readonly IEventRepository _ieventRepository;
        private readonly ITournamentStatkeeperRepository _itournamentStatkeeperRepository;
        private readonly IImagesRepository _iimagesRepository;
        private readonly ITournamentResultRepository _itournamentResultRepository;
        private readonly IRoleRepository _iroleRepository;
        private readonly ITournamentTypeRepository _itournamentTypeRepository;
        private readonly IUserInfoRepository _iUserInfoRepository;
        private readonly ITeamMemberRepository _iteamMemberRepository;
        private readonly IEventCollectionRepository _ieventCollectionRepository;
        private readonly ITournamentEventCollectionRepository _itournamentEventCollectionRepository;
        private readonly IBranchSchedulingRepository _ibranchSchedulingRepository;
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly IActivityRepository _iactivityRepository;
        public TournamentService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _context = context;
            _itournamentRepository = new TournamentRepository(context);
            _itournamentTeamRepository = new TournamentTeamRepository(context);
            _itournamentVenueRepository = new TournamentVenueRepository(context);
            _ieventCollectionTeamRepository = new EventCollectionTeamRepository(context);
            _ieventRepository = new EventRepository(context);
            _itournamentStatkeeperRepository = new TournamentStatkeeperRepository(context);
            _iimagesRepository = new ImagesRepository(context);
            _itournamentResultRepository = new TournamentResultRepository(context);
            _iroleRepository = new RoleRepository(context);
            _itournamentTypeRepository = new TournamentTypeRepository(context);
            _iUserInfoRepository = new UserInfoRepository(context);
            _iteamMemberRepository = new TeamMemberRepository(context);
            _ieventCollectionRepository = new EventCollectionRepository(context);
            _itournamentEventCollectionRepository = new TournamentEventCollectionRepository(_context);
            _ibranchSchedulingRepository = new BranchSchedulingRepository(_context);
            _userInfoRepository = new UserInfoRepository(context);
            _iactivityRepository = new ActivityRepository(context);
        }
        /// <summary>
        /// create tournament
        /// </summary>
        /// <param name="tournament">details of tournament</param>
        /// <returns>tournament object or error object</returns>
        public async Task<Domain.Entities.Tournament> CreateAsync(Domain.Entities.Tournament tournament)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            tournament = await _iunitOfWork.Repository<Domain.Entities.Tournament>().AddAsync(tournament);
            await _iunitOfWork.Commit(cancellationToken);
            return tournament;
        }

        /// <summary>
        /// create tournament by mapping different info
        /// </summary>
        /// <param name="createTournament">basic information of tournament</param>
        /// <param name="email">user email who is creating the tournament</param>
        /// <param name="userId">user id who is creating the tournament</param>
        /// <returns>string success message or error object</returns>
        public async Task<ErrorOr<GenericMessage>> CreateTournamentAsync(CreateTournament createTournament, string email, long userId)
        {
            TournamentMapper tournamentMapper = new TournamentMapper();

            try
            {
                var eventTypeList = await _ieventRepository.GetEventTypeAsync();
                var regulationType = eventTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventType.Regulation.ToString().ToLower()).FirstOrDefault();
                var activity = await _iactivityRepository.GetActivityByEventTypeAndActivityCategoryId(regulationType.Id, createTournament.ActivityId);
                createTournament.ActivityId = activity.Id;

                Domain.Entities.TournamentType tournamentType = await _itournamentTypeRepository.GetTournamentTypeByNameAsync(Domain.Enums.Enum.TournamentType.SingleElimination.ToString());
                Domain.Entities.Tournament tournament = tournamentMapper.MapCreateTournament(createTournament, email, tournamentType);
                tournament = await CreateAsync(tournament);

                List<TournamentVenue> tournamentVenueList = tournamentMapper.MapCreateTournamentVenue(createTournament, email, tournament.Id);
                List<TournamentStatkeeper> tournamentStatkeeperList = tournamentMapper.MapCreateTournamentStatkeeper(createTournament, email, tournament.Id);

                tournamentStatkeeperList = await _itournamentVenueRepository.CreateTournamentStatkeeperListAsync(tournamentStatkeeperList);
                tournamentVenueList = await _itournamentVenueRepository.CreateTournamentVenueListAsync(tournamentVenueList);


                _logger.LogInformation("Tournament created successfully @{tournament}", tournament);

                return new GenericMessage { Message = Constants.APIErrorMessages.TOURNAMENT_CREATED_SUCESSFULLY };
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        /// <summary>
        /// Get tournament details by id
        /// </summary>
        /// <param name="id">Tournament id</param>
        /// <param name="email">user email who is getting the tournament</param>
        /// <param name="userId">user id who is getting the tournament</param>
        /// <returns>object of tournament basic info in case of success message or error object</returns>
        public async Task<ErrorOr<GetTournament>> GetTournamentByIdAsync(long id, string email, long userId)
        {
            TournamentMapper tournamentMapper = new TournamentMapper();
            try
            {
                Domain.Entities.Tournament tournament = await _itournamentRepository.GetTournamentByIdAsync(id, userId);

                if (tournament is not null)
                {
                    Domain.Entities.UserInfo userInfo = _iUserInfoRepository.GetUserInfoByUserEmail(tournament.CreatedBy);
                    Domain.Entities.Images image = await _iimagesRepository.GetUserProfileImage(userInfo.Id);

                    GetTournament getTournament = await _itournamentRepository.GetTournamentByIdJoinAsync(id);
                    string checkForTournamentIsJoinedByUserAnyTeam = await _itournamentRepository.CheckForTournamentIsJoinedByUserAnyTeam(id, userId);
                    bool ifThisUserIsTheCaptainOfTheTeamJoinedInTheTournament = await _itournamentRepository.CheckForTournamentIsCaptainByUserAnyTeam(id, userId);

                    getTournament.CreatedBy = userInfo.Name;
                    getTournament.CreatedByImage = image != null ? image.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
                    getTournament.JoinedStatus = checkForTournamentIsJoinedByUserAnyTeam;
                    getTournament.IsCaptain = ifThisUserIsTheCaptainOfTheTeamJoinedInTheTournament;

                    _logger.LogInformation("Get tournament reponse @{getTournament}", getTournament);
                    return getTournament is not null ? getTournament : Domain.Common.Errors.Errors.Tournament.NoVenueFound;
                }
                else
                {
                    _logger.LogInformation("No tournament found");
                    return Domain.Common.Errors.Errors.Tournament.NoTournamentFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// Get the list of tournaments
        /// </summary>
        /// <param name="email">user email who is getting the list tournament</param>
        /// <param name="userId">user id who is getting the list tournament</param>
        /// <returns>list of tournament in case of success and error object in cse of error</returns>
        public async Task<ErrorOr<PagedResponse<List<GetAllTournament>>>> GetAllTournamentAsync(string email, long userId, BasicFilter paginationFilter, TournamentStatus tournamentStatus, long teamId,
            long roleId, long venueId, long activityCategoryId)
        {
            try
            {
                var Role = await _iroleRepository.GetRole(userId, roleId);

                if (Role == Domain.Enums.Enum.Roles.Business.ToString())
                {
                    List<GetAllTournament> getAllTournamentList = await _itournamentRepository.GetAllTournamentAsyncByBusiness(userId, paginationFilter, tournamentStatus, teamId, venueId);
                    int TotalRecords = await _itournamentRepository.GetAllTournamentCountAsyncByBusiness(userId, tournamentStatus, teamId, venueId);

                    return new PagedResponse<List<GetAllTournament>>(TotalRecords, getAllTournamentList, paginationFilter.PageNumber, paginationFilter.PageSize);
                }
                else if (Role == Domain.Enums.Enum.Roles.Player.ToString())
                {

                    IQueryable<Domain.Entities.Tournament> tournamentTbl = _itournamentRepository.SearchInTournament(paginationFilter.sSearch);

                    // IQueryable<Domain.Entities.Tournament> tournamentTbl = _itournamentRepository.GetAll(activityCategoryId,paginationFilter.sSearch);
                    IQueryable<TournamentTeam> tournamentTeamList = _itournamentTeamRepository.GetAll();
                    IQueryable<TournamentVenue> tournamentVenueList = _itournamentVenueRepository.GetAll();

                    IQueryable<long> tournamentActivityIdList = tournamentTbl.Select(x => x.ActivityId).AsQueryable();
                    IQueryable<string> tournamentCreatedByList = tournamentTbl.Select(x => x.CreatedBy).AsQueryable();
                    IQueryable<long> teamIdList = _iteamMemberRepository.GetTeamsIdByPlayerIdList(userId);

                    IQueryable<Domain.Entities.Images> activityImageList = _iimagesRepository.GetTournamentActivitiesImages(tournamentActivityIdList);

                    IQueryable<Domain.Entities.UserInfo> userInfo = _iUserInfoRepository.GetUserInfoByUserEmailList(tournamentCreatedByList);
                    IQueryable<long> createdByIdList = userInfo.Select(x => x.Id).AsQueryable();
                    IQueryable<Domain.Entities.Images> createdByImageList = _iimagesRepository.GetUserProfileImages(createdByIdList);

                    List<GetAllTournament> getAllTournamentList = await _itournamentRepository.GetAllTournamentAsyncByPlayer(userId, paginationFilter, tournamentStatus, teamId, venueId, tournamentTbl, tournamentTeamList
                       , tournamentVenueList, activityImageList, userInfo, teamIdList, createdByImageList);

                    int TotalRecords = await _itournamentRepository.GetAllTournamentCountAsyncByPlayer(userId, tournamentStatus, teamId, venueId, tournamentTbl, tournamentTeamList
                     , tournamentVenueList, activityImageList, userInfo, teamIdList, createdByImageList, paginationFilter.sSearch);

                    return new PagedResponse<List<GetAllTournament>>(TotalRecords, getAllTournamentList, paginationFilter.PageNumber, paginationFilter.PageSize);


                }
                else if (Role == Domain.Enums.Enum.Roles.Statkeeper.ToString())
                {
                    List<GetAllTournament> getAllTournamentList = await _itournamentRepository.GetAllTournamentAsyncByStatkeeper(userId, paginationFilter, tournamentStatus, teamId, venueId);
                    int TotalRecords = await _itournamentRepository.GetAllTournamentCountAsyncByStatkeeper(userId, tournamentStatus, teamId, venueId);
                    return new PagedResponse<List<GetAllTournament>>(TotalRecords, getAllTournamentList, paginationFilter.PageNumber, paginationFilter.PageSize);
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
        /// request by team to join tournament
        /// </summary>
        /// <param name="email">user email who is requesting to join tournament</param>
        /// <param name="userId">user id who is requesting to join tournament</param>
        /// <param name="tournamentId">tournament id</param>
        /// <param name="teamId">team id</param>
        /// <returns>string success message or error object</returns>
        public async Task<ErrorOr<RequestToJoinResponse>> RequestToJoinAsync(string email, long userId, long tournamentId, long teamId)
        {
            TournamentMapper tournamentMapper = new TournamentMapper();
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {
                Domain.Entities.UserInfo userInfo = _userInfoRepository.GetUserInfoByUserId(userId);
                var checkTournamentActivityMatchesTeamActivity = await _itournamentRepository.CheckTournamentActivityMatchesTeamActivityAsync(tournamentId, teamId);
                var checkTeamAlreadyRequestTheTournament = await _itournamentTeamRepository.CheckTournamentTeamRequestAsync(tournamentId, teamId);
                var CheckTournamentTeamRequestAcceptedCount = _itournamentTeamRepository.CheckTournamentTeamRequestAcceptedCountAsync(tournamentId);
                var tournament = await _iunitOfWork.Repository<Domain.Entities.Tournament>().GetByIdAsync(tournamentId);
                var teamMembersCount = _iteamMemberRepository.GetTeamMemberRequestAcceptedCount(teamId);
                var tournamentBusinesId = _itournamentTeamRepository.GetTournamentBusinesId(tournamentId);

                // var tournamentBusinnesId = await _context.TournamentVenue.Where(x => x.TournamentId == tournamentId).Select(x => x.Venue.Branch.UserInfoId).FirstOrDefaultAsync();
                //List<RepeatedJersey> repeatedJersey = await _itournamentTeamRepository.GetRepeatedJersey(teamId);
                //if (repeatedJersey.Any())
                //{
                //    var groupedByJerseyNo = repeatedJersey.GroupBy(x => x.JerseyNo);
                //    var message = new StringBuilder();
                //    var groupedByJerseylastNo = groupedByJerseyNo.Last();
                //    string groupedByJerseylastNokey = groupedByJerseylastNo.Key;

                //    foreach (var group in groupedByJerseyNo)
                //    {
                //        string groupKeys = group.Key;
                //        var ids = group.Select(x => x.Id);
                //        var names = group.Select(x => x.Name);

                //        var playerIdString = string.Join(", ", ids);
                //        var playerNameString = string.Join(", ", names);

                //        message.Append($"PlayerId {playerIdString} has the same Jersey number {group.Key}");
                //        if (groupKeys != groupedByJerseylastNokey)
                //        {
                //            message.Append(" and ");
                //        }
                //    }

                //    return new RequestToJoinResponse { Message = message.ToString() };
                //}

                if (tournament is null)
                {
                    _logger.LogWarning("Tournament not found.");
                    return Domain.Common.Errors.Errors.Tournament.NoTournamentFound;
                }
                else if (CheckTournamentTeamRequestAcceptedCount == tournament.SeasonTeamCapacity) // --> teamCapacity change to teamCapacity
                {
                    _logger.LogWarning("Tournament request limit reached.");
                    return Domain.Common.Errors.Errors.Tournament.RequestLimitReached;
                }
                else if (teamMembersCount <= tournament.MinPlayerPerTeam && teamMembersCount >= tournament.MaxPlayerPerTeam)
                {
                    _logger.LogWarning("Team Members count is not valid.");
                    return Domain.Common.Errors.Errors.Tournament.TeamMemberCountIsNotValid;
                }
                if (!checkTournamentActivityMatchesTeamActivity)
                {
                    _logger.LogWarning("Tournament activity did not match team activity");
                    return Domain.Common.Errors.Errors.Tournament.YouCanNotJoinTournament;
                }
                else if (checkTeamAlreadyRequestTheTournament is not null && checkTeamAlreadyRequestTheTournament.IsAccepted != true)
                {
                    _logger.LogWarning("Team request is pending {@checkTeamAlreadyRequestTheTournament}", checkTeamAlreadyRequestTheTournament);
                    return Domain.Common.Errors.Errors.Tournament.YourRequestIsPending;
                }
                else if (checkTeamAlreadyRequestTheTournament is not null && checkTeamAlreadyRequestTheTournament.IsAccepted != false)
                {
                    _logger.LogWarning("Team request is accepted {@checkTeamAlreadyRequestTheTournament}", checkTeamAlreadyRequestTheTournament);
                    return Domain.Common.Errors.Errors.Tournament.YourRequestIsAccepted;
                }
                else
                {
                    NotificationMapper notificationMapper = new NotificationMapper();
                    Domain.Entities.TournamentTeam tournamentTeam = tournamentMapper.MapTournamentTeamForCreate(tournamentId, teamId, email);
                    tournamentTeam = await _iunitOfWork.Repository<Domain.Entities.TournamentTeam>().AddAsync(tournamentTeam);
                    var teamName = await _context.Team.Where(x => x.Id == teamId).Select(x => x.Name).FirstOrDefaultAsync();

                    //creating notification

                    Common.GetNotificationMsg notificationMessage = Common.GetNotificationMessages.GetNotificationMessage.JoinToTournamentRequest(tournament.Name, teamName);
                    var notification = tournamentMapper.MapTournamentWithNotification(email, userInfo.Id);
                    var notify = notificationMapper.CreateNotification(notificationMessage, Domain.Enums.Enum.NotificationPlatform.Mobile, Domain.Enums.Enum.NotificationsTypes.TeamJoinRequest, email);
                    notify = await _iunitOfWork.Repository<Notifications>().AddAsync(notify);
                    await _iunitOfWork.Commit(cancellationToken);

                    var notifyTo = notificationMapper.CreateNotificationTo(notify, tournamentBusinesId, email, userId);
                    await _iunitOfWork.Repository<Domain.Entities.NotificationTo>().AddAsync(notifyTo);
                    await _iunitOfWork.Commit(cancellationToken);

                 //  _logger.LogInformation("TeamMember is created successfully {@teamMember}", teamMember);

                    _logger.LogInformation("Request is successfully delivered");
                    return new RequestToJoinResponse { Message = Constants.APIErrorMessages.REQUEST_CREATED_SUCESSFULLY };
                }
            } 
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// accept or reject request to join tournament
        /// </summary>
        /// <param name="email">user email who is accept or reject tournament request</param>
        /// <param name="userId">user id who is accept or reject tournament request</param>
        /// <param name="tournamentId">tournament id</param>
        /// <param name="teamId">team id</param>
        /// <param name="accept">true = accept and false = reject</param>
        /// <returns>string success message or error object</returns>
        public async Task<ErrorOr<AcceptRequestToJoinResponse>> AcceptRequestToJoinAsync(string email, long userId, long tournamentId, long teamId, bool accept)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                Domain.Entities.TournamentTeam tournamentTeam = teamId != 0 ? await _itournamentRepository.GetTournamentTeamByIdsAsync(tournamentId, teamId) :
                                                                 await _itournamentTeamRepository.GetUserTeamJoinedInTheTournamentAsync(tournamentId, userId);
                if (tournamentTeam is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Tournament.NoTeamRequestFound}");
                    return Domain.Common.Errors.Errors.Tournament.NoTeamRequestFound;
                }
                if (accept)
                {
                    tournamentTeam.IsAccepted = accept;
                    await _iunitOfWork.Repository<Domain.Entities.TournamentTeam>().UpdateAsync(tournamentTeam);
                    await _iunitOfWork.Commit(cancellationToken);
                    _logger.LogInformation("Tournament team updated sucessfully {@tournamentTeam}", tournamentTeam);

                    return new AcceptRequestToJoinResponse { Message = Constants.APIErrorMessages.REQUEST_ACCEPTED_SUCESSFULLY };
                }
                else
                {
                    await _iunitOfWork.Repository<Domain.Entities.TournamentTeam>().DeleteAsync(tournamentTeam);
                    await _iunitOfWork.Commit(cancellationToken);
                    _logger.LogInformation("Tournament team delete sucessfully {@tournamentTeam}", tournamentTeam);

                    return new AcceptRequestToJoinResponse { Message = Constants.APIErrorMessages.REQUEST_REJECTED_SUCESSFULLY };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// Randomly schedule matches between tournament teams
        /// </summary>
        /// <param name="email">user email who is requesting</param>
        /// <param name="userId">user id who is requesting</param>
        /// <param name="tournamentId">this tournament teams will be randomize</param>
        /// <returns></returns>
        public async Task<ErrorOr<string>> RandomizeTeamAsync(string email, long userId, long tournamentId)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            EventCollectionMapper eventCollectionMapper = new EventCollectionMapper();
            EventCollectionTeamMapper eventCollectionTeamMapper = new EventCollectionTeamMapper();
            EventMapper eventMapper = new EventMapper();
            EventTeamMapper eventTeamMapper = new EventTeamMapper();
            TournamentEventCollectionMapper tournamentEventCollectionMapper = new TournamentEventCollectionMapper();

            try
            {
                List<TournamentTeam> tournamentTeamList = await _itournamentTeamRepository.GetTournamentTeamAllRequestAcceptedAsync(tournamentId);
                Domain.Entities.Tournament tournament = await _itournamentRepository.GetTournamentByIdAsync(tournamentId, userId);
                List<TournamentVenue> tournamentVenueList = await _itournamentVenueRepository.GetAllTournamentVenueAsync(tournamentId);
                List<long> branchIdList = tournamentVenueList.Select(x => x.Venue.BranchId.Value).ToList();
                List<BranchScheduling> branchSchedulingList = await _ibranchSchedulingRepository.GetBranchSchedulingByBranchIdList(branchIdList);
                var venueStatkeeperList = await _iunitOfWork.Repository<Domain.Entities.VenuesStatskeeper>().GetAllAsync();

                List<AllStatkeepersVenueList> tournamentStatkeeperList = await _itournamentStatkeeperRepository.GetStatkeeprList(tournamentId);


                if (tournament is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Tournament.NoTournamentFound}");
                    return Domain.Common.Errors.Errors.Tournament.NoTournamentFound;
                }
                else if (tournamentTeamList.Any() && tournamentTeamList.Count() < tournament.TeamCapacity)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Tournament.NotEnoughTeamsToRandomize}");
                    return Domain.Common.Errors.Errors.Tournament.NotEnoughTeamsToRandomize;
                }
                else if (!tournamentVenueList.Any())
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Tournament.NotEnoughVenuesToRandomize}");
                    return Domain.Common.Errors.Errors.Tournament.NotEnoughVenuesToRandomize;
                }
                else
                {
                    List<long> teamList = tournamentTeamList.Select(t => t.Team.Id).ToList();
                    List<long> venueList = tournamentVenueList.Select(t => t.Venue.Id).ToList();

                    RandomizeTeams randomizeTeams = new RandomizeTeams(_logger);

                    randomizeTeams.TeamIdList = teamList;
                    randomizeTeams.TournamentVenueList = tournamentVenueList.Where(x => venueStatkeeperList.Select(x => x.VenueId).Contains(x.VenueId)).Select(x => x.Venue).ToList();
                    randomizeTeams.TournamentStatkeeperIdList = tournamentStatkeeperList.Select(x => x.UserId).ToList();
                    randomizeTeams.VenueStatskeeperList = venueStatkeeperList;
                    randomizeTeams.Tournament = tournament;
                    randomizeTeams.BranchSchedulingList = branchSchedulingList;

                    var validationCheck = randomizeTeams.ValidationCheckForSchedule();

                    if (validationCheck)
                    {
                        var seasonSchedulingResponseList = randomizeTeams.ScheduleSeasonMatches();

                        _logger.LogInformation("seasonSchedulingResponseList {@seasonSchedulingResponseList}", seasonSchedulingResponseList);
                        List<long> venueIdForEventCollection = seasonSchedulingResponseList.Select(x => x.VenueId).Distinct().ToList();

                        for (int i = 0; i < venueIdForEventCollection.Count; i++)
                        {
                            var venueMatchList = seasonSchedulingResponseList.Where(x => x.VenueId == venueIdForEventCollection[i]).ToList();

                            for (int j = 0; j < venueMatchList.Count; j++)
                            {
                                var getFirstMatch = venueMatchList[j];

                                List<TournamentEventCollection> tournamentEventCollectionList = await _itournamentEventCollectionRepository.GetTournamentEventCollectionByTournamentIdList(tournament.Id);

                                var getEventCollectionIfWeHaveAlready = tournamentEventCollectionList
                                                                        .Where(x => x.EventCollection.VenueId == getFirstMatch.VenueId &&
                                                                        x.EventCollection.StatKeeperId == getFirstMatch.StatkeeperId &&
                                                                        x.EventCollection.EventType.Name.ToLower() == Domain.Enums.Enum.EventType.Regulation.ToString().ToLower())
                                                                        .FirstOrDefault();

                                using (var dbContextTransaction = _context.Database.BeginTransaction())
                                {
                                    if (getEventCollectionIfWeHaveAlready is not null)
                                    {
                                        List<long> matchTeamList = getFirstMatch.EventTeamIdList;

                                        List<EventCollectionTeam> eventCollectionTeamList = eventCollectionTeamMapper.MapEventCollectionTeamListForCreate(email, matchTeamList, getEventCollectionIfWeHaveAlready.EventCollectionId);
                                        eventCollectionTeamList = await _ieventCollectionTeamRepository.CreateEventCollectionTeamList(eventCollectionTeamList);

                                        Domain.Entities.Event @event = eventMapper.MapEventForCreate(email, getFirstMatch.StatkeeperId, getFirstMatch.EventDateTime);

                                        @event = await _iunitOfWork.Repository<Domain.Entities.Event>().AddAsync(@event);
                                        await _iunitOfWork.Commit(cancellationToken);

                                        Domain.Entities.EventStatus eventStatus = await _ieventRepository.GetEventStatusByNameAsync(Domain.Enums.Enum.EventStatus.InActive.ToString());

                                        List<EventTeam> eventTeamList = eventTeamMapper.CreateEventTeamSeasonList(email, eventCollectionTeamList, @event.Id, eventStatus.Id);
                                        eventTeamList = await _ieventRepository.CreateEventTeamList(eventTeamList);
                                    }
                                    else
                                    {
                                        var eventTypeList = await _iunitOfWork.Repository<Domain.Entities.EventType>().GetAllAsync();
                                        var eventTypeId = eventTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventType.Regulation.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                                        EventCollection eventCollection = eventCollectionMapper.MapEventCollectionForCreate(getFirstMatch.VenueId, tournament.ActivityId, eventTypeId, email, getFirstMatch.StatkeeperId);

                                        eventCollection = await _iunitOfWork.Repository<Domain.Entities.EventCollection>().AddAsync(eventCollection);
                                        await _iunitOfWork.Commit(cancellationToken);

                                        TournamentEventCollection tournamentEventCollection = tournamentEventCollectionMapper.MapEventCollectionWithTournament(eventCollection, tournament.Id, email);

                                        tournamentEventCollection = await _iunitOfWork.Repository<Domain.Entities.TournamentEventCollection>().AddAsync(tournamentEventCollection);
                                        await _iunitOfWork.Commit(cancellationToken);

                                        List<long> matchTeamList = getFirstMatch.EventTeamIdList;

                                        List<EventCollectionTeam> eventCollectionTeamList = eventCollectionTeamMapper.MapEventCollectionTeamListForCreate(email, matchTeamList, eventCollection.Id);
                                        eventCollectionTeamList = await _ieventCollectionTeamRepository.CreateEventCollectionTeamList(eventCollectionTeamList);

                                        Domain.Entities.Event @event = eventMapper.MapEventForCreate(email, getFirstMatch.StatkeeperId, getFirstMatch.EventDateTime);

                                        @event = await _iunitOfWork.Repository<Domain.Entities.Event>().AddAsync(@event);
                                        await _iunitOfWork.Commit(cancellationToken);

                                        Domain.Entities.EventStatus eventStatus = await _ieventRepository.GetEventStatusByNameAsync(Domain.Enums.Enum.EventStatus.InActive.ToString());

                                        List<EventTeam> eventTeamList = eventTeamMapper.CreateEventTeamSeasonList(email, eventCollectionTeamList, @event.Id, eventStatus.Id);
                                        eventTeamList = await _ieventRepository.CreateEventTeamList(eventTeamList);
                                    }
                                    dbContextTransaction.Commit();
                                }
                            }
                        }

                        return Constants.APIErrorMessages.MATCHES_RANDOMIZE_SUCESSFULLY;
                    }
                    else
                    {
                        return Domain.Common.Errors.Errors.Tournament.NotEnoughVenuesToRandomize;
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
        /// Get Statkeeper List by tournament
        /// </summary>
        /// <param name="email">user email who is requesting</param>
        /// <param name="userId">user id who is requesting</param>
        /// <param name="tournamentId">tournaments statskeeper</param>
        /// <returns>list of statkeeper</returns>
        public async Task<ErrorOr<List<AllStatkeepersVenueList>>> GetStatkeeperByIdAsync(string email, long userId, long tournamentId)
        {
            TournamentStatkeeperMapper tournamentStatkeeperMapper = new TournamentStatkeeperMapper();
            try
            {
                List<AllStatkeepersVenueList> allStatkeepersDetailList = await _itournamentStatkeeperRepository.GetStatkeeprList(tournamentId);
                if (allStatkeepersDetailList.Any())
                {
                    List<Domain.Entities.Images> imageList = await _iimagesRepository.GetImageByUserIdList(allStatkeepersDetailList.Select(x => x.UserId).ToList());
                    allStatkeepersDetailList = tournamentStatkeeperMapper.MapImagesForTournamentStatkeeper(allStatkeepersDetailList, imageList);

                    _logger.LogInformation("Statkeeper list {@allStatkeepersDetailList}", allStatkeepersDetailList);
                    return allStatkeepersDetailList;
                }
                else
                {
                    _logger.LogWarning("No Statkeeper Found");
                    return Domain.Common.Errors.Errors.Tournament.NoStatkeeperFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<List<GenericObj>>> GetStatkeeperListByTournamentAsync(string email, long userId, long tournamentId)
        {
            try
            {
                return await _itournamentRepository.GetStatkeeperListByTournamentIdAsync(tournamentId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<List<TeamGenericObj>>> GetTournamentTeamsAsync(string email, long userId, long tournamentId, long roleId)
        {
            try
            {
                var results = await _itournamentRepository.GetTournamentTeamsListAsync(tournamentId, userId);

                return results.Any() ? results : Domain.Common.Errors.Errors.Tournament.NoTournamentTeamFound;

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<PagedResponse<List<RequestedTeamsInTournament>>>> GetTournamentTeamsRequestAsync(BasicFilter paginationFilter, string email, long userId, long tournamentId)
        {
            try
            {
                List<RequestedTeamsInTournament> teamRequestList = await _itournamentRepository.GetTournamentTeamsRequestAsync(paginationFilter, tournamentId);
                int TotalRecords = await _itournamentRepository.GetTournamentTeamsRequestAsyncCount(tournamentId);
                return new PagedResponse<List<RequestedTeamsInTournament>>(TotalRecords, teamRequestList, paginationFilter.PageNumber, paginationFilter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<GetTournamentEvent>> GetTournamentEventsByIdAsync(string email, long userId, long tournamentId)
        {
            TournamentMapper tournamentMapper = new TournamentMapper();
            try
            {
                Domain.Entities.Tournament tournament = await _iunitOfWork.Repository<Domain.Entities.Tournament>().GetByIdAsync(tournamentId);
                if (tournament is null)
                {
                    return Domain.Common.Errors.Errors.Tournament.NoTournamentFound;
                }
                else
                {
                    //true means season is finished and false means season is still in progress
                    bool checkIfTheSeasonIsInProgressOrFinished = await _itournamentRepository.GetTournamentFormatByTournamentId(tournamentId, userId);

                    List<Application.Response.Tournament.Event> eventDetailList = await _itournamentRepository.GetTournamentEventsByIdAsync(tournamentId);
                    GetTournamentEvent getTournamentEvent = tournamentMapper.MapTournamentEventListWithTournament(tournament, eventDetailList, checkIfTheSeasonIsInProgressOrFinished);

                    return getTournamentEvent.EventDetailList.Any() ? getTournamentEvent : Domain.Common.Errors.Errors.Tournament.NoEventsFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// Returns the tournament results
        /// </summary>
        /// <param name="email">user email who is requesting the endpoint</param>
        /// <param name="userId">user id who is requesting the endpoint</param>
        /// <param name="tournamentId">Tournament id of the results</param>
        /// <returns>problem statement in case of error and details in case of success</returns>
        public async Task<ErrorOr<List<GetTournamentResult>>> GetTournamentResultByIdAsync(string email, long userId, long tournamentId)
        {
            try
            {
                List<GetTournamentResult> getTournamentResultList = await _itournamentResultRepository.GetTournamentResultByTournamentIdAsync(tournamentId, userId);
                _logger.LogInformation("getTournamentResultList {@getTournamentResultList}", getTournamentResultList);
                return getTournamentResultList.Any() ? getTournamentResultList : Domain.Common.Errors.Errors.Tournament.NoTournamentResultFound;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// get the tournament playoff tree view structured matches
        /// </summary>
        /// <param name="email">user email who is requesting</param>
        /// <param name="userId">user id whi is requesting</param>
        /// <param name="tournamentId">tournament which is requested</param>
        /// <returns>in case of error returns problem statement and in success return model</returns>
        public async Task<ErrorOr<List<PlayoffGroups>>> GetTournamentPlayoffByIdAsync(string email, long userId, long tournamentId)
        {
            TournamentMapper tournamentMapper = new TournamentMapper();

            try
            {
                List<TournamentPlayoffResponse> tournamentPlayoffResponseList = await _itournamentRepository.GetTournamentPlayoffByIdAsync(tournamentId);

                if (!tournamentPlayoffResponseList.Any())
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Tournament.NoEventsFound}");
                    return Domain.Common.Errors.Errors.Tournament.NoEventsFound;
                }
                else
                {
                    int tournamentTeamCount = _itournamentTeamRepository.CheckTournamentTeamRequestAcceptedCountAsync(tournamentId);
                    List<PlayoffGroups> playoffGroupList = tournamentMapper.MapTournamentPlayOffResponse(tournamentPlayoffResponseList, tournamentTeamCount);

                    _logger.LogInformation("playoffGroupList {@playoffGroupList}", playoffGroupList);
                    return playoffGroupList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<string>> DeleteTournamentStatkeeperAsync(string email, long userId, long tournamentId, int statkeeperId)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {
                TournamentStatkeeper tournamentStatkeeper = await _itournamentStatkeeperRepository.GetTournamentStatkeeperByIdsAsync(tournamentId, statkeeperId);

                if (tournamentStatkeeper is null)
                {
                    return Domain.Common.Errors.Errors.Tournament.NoStatkeeperFound;
                }
                else
                {
                    tournamentStatkeeper.IsActive = false;
                    tournamentStatkeeper.IsDeleted = true;

                    await _iunitOfWork.Repository<Domain.Entities.TournamentStatkeeper>().UpdateAsync(tournamentStatkeeper);
                    await _iunitOfWork.Commit(cancellationToken);

                    return Constants.APIErrorMessages.STATKEEPER_DELETED_SUCESSFULLY;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<string>> AddStatkeeperAsync(string email, long userId, long tournamentId, int statkeeperId)
        {
            TournamentStatkeeperMapper tournamentStatkeeperMapper = new TournamentStatkeeperMapper();
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                Domain.Entities.Tournament tournament = await _itournamentRepository.GetTournamentByIdAsync(tournamentId, userId);
                if (tournament is null)
                {
                    _logger.LogInformation("No tournament found");
                    return Domain.Common.Errors.Errors.Tournament.NoTournamentFound;
                }
                else
                {
                    TournamentStatkeeper tournamentStatkeeper = tournamentStatkeeperMapper.MapTournamentStatkeeperForCreate(email, tournamentId, statkeeperId);

                    tournamentStatkeeper = await _iunitOfWork.Repository<Domain.Entities.TournamentStatkeeper>().AddAsync(tournamentStatkeeper);
                    await _iunitOfWork.Commit(cancellationToken);
                    _logger.LogInformation("Statkeeper added to the tournament successfully");
                    return Constants.APIErrorMessages.STATKEEPER_TOURNAMENT_SUCESSFULLY;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// generate random matches between the teams between tournament start and end date on random venues
        /// </summary>
        /// <param name="startDate">tournament start date</param>
        /// <param name="endDate">tournament end date</param>
        /// <param name="teams">list of teams in the tournament</param>
        /// <param name="venues">list of venues in the tournament</param>
        /// <returns></returns>
        private List<Tuple<DateTime, long, long, long>> GenerateMatches(DateTime startDate, DateTime endDate, List<long> teams, List<long> venues)
        {
            // Shuffle the list randomly
            Random rand = new Random();
            teams = teams.OrderBy(x => rand.Next()).ToList();

            // Create the list of matches
            List<Tuple<DateTime, long, long, long>> matches = new List<Tuple<DateTime, long, long, long>>();

            for (int i = 0; i < teams.Count; i += 2)
            {
                // Generate a random date within the given range
                TimeSpan timeSpan = endDate - startDate;
                int daysToAdd = rand.Next(timeSpan.Days);
                DateTime matchDate = startDate.AddDays(daysToAdd);

                // Choose a random venue for the match
                long venue = venues[rand.Next(venues.Count)];

                // Add match to the list of matches
                matches.Add(Tuple.Create(matchDate, teams[i], teams[i + 1], venue));
            }

            // Sort matches by date
            matches = matches.OrderBy(x => x.Item1).ToList();

            return matches;
        }
        /// <summary>
        /// return the tournament prizes
        /// </summary>
        /// <param name="tournamentId">tournament id</param>
        /// <returns>problem statement in case of error and list of tournament prizes in case of success</returns>
        public async Task<ErrorOr<List<TournamentPrizeRes>>> GetTournamentPrizes(long tournamentId)
        {
            try
            {
                List<TournamentPrizeRes> tournamentPrize = new List<TournamentPrizeRes>();
                tournamentPrize = await _itournamentRepository.TournamentPrizeQuery(tournamentId);
                return tournamentPrize;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<string>> updatePrizeReq(UpdatePrizeReq updatePrizeReq)
        {
            try
            {
                _logger.LogInformation("UpdatePrize");

                TournamentMapper mapper = new TournamentMapper();
                var tournament = await _iunitOfWork.Repository<Domain.Entities.Tournament>().GetByIdAsync(updatePrizeReq.Id);
                tournament = mapper.UpdatePrizeMapper(tournament, updatePrizeReq);

                CancellationToken cancellationToken = CancellationToken.None;
                await _iunitOfWork.Repository<Domain.Entities.Tournament>().UpdateAsync(tournament);
                await _iunitOfWork.Commit(cancellationToken);

                _logger.LogInformation("Tournament updated succesfully ", tournament);
                return Constants.APIErrorMessages.RECORD_UPDATED;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<GenericMessage>> UpdateTournament(UpdateTournamentReq updateTournament, string email)
        {
            try
            {
                CancellationToken cancellationToken = CancellationToken.None;
                _logger.LogInformation("UpdateTournament");


                TournamentMapper mapper = new TournamentMapper();
                var tournament = await _iunitOfWork.Repository<Domain.Entities.Tournament>().GetByIdAsync(updateTournament.Id);
                var activityId =  _itournamentRepository.getActivityId(updateTournament.ActivityId);
                tournament = mapper.UpdateTournament(tournament, updateTournament, activityId);
                var getTournamentstatkeeper = _itournamentRepository.getTournamentstatkeeper(updateTournament);
                var tournamentStatkeeprTemp = _context.TournamentStatkeeper.Where(x => x.TournamentId == updateTournament.Id).ToList();
                foreach (var item in getTournamentstatkeeper)
                {
                    await _iunitOfWork.Repository<TournamentStatkeeper>().DeleteAsync(item);
                }
                foreach (var item in updateTournament.StatkeeperIdList)
                {
                    TournamentStatkeeper tournamentStatskeeper = new TournamentStatkeeper();

                    tournamentStatskeeper.TournamentId = updateTournament.Id;
                    tournamentStatskeeper.CreatedDateTime = DateTime.Now;
                    tournamentStatskeeper.LastModifiedDateTime = DateTime.Now;
                    tournamentStatskeeper.LastModifiedBy = email;
                    tournamentStatskeeper.CreatedBy = email;
                    tournamentStatskeeper.StatkeeperId = item;

                    await _iunitOfWork.Repository<TournamentStatkeeper>().AddAsync(tournamentStatskeeper);
                }

                var getTournamentVenue = _itournamentRepository.getTournamentVenue(updateTournament);
                foreach (var item in getTournamentVenue)
                {
                    await _iunitOfWork.Repository<TournamentVenue>().DeleteAsync(item);
                }
                foreach (var item in updateTournament.VenueIdList)
                {
                    TournamentVenue tournamentVenue = new TournamentVenue();

                    tournamentVenue.TournamentId= updateTournament.Id;
                    tournamentVenue.CreatedDateTime = DateTime.Now;
                    tournamentVenue.LastModifiedDateTime = DateTime.Now;
                    tournamentVenue.LastModifiedBy = email;
                    tournamentVenue.CreatedBy = email;
                    tournamentVenue.VenueId = item;

                    await _iunitOfWork.Repository<TournamentVenue>().AddAsync(tournamentVenue);
                }

                await _iunitOfWork.Repository<Domain.Entities.Tournament>().UpdateAsync(tournament);
                await _iunitOfWork.Commit(cancellationToken);

                _logger.LogInformation("Tournament  updated successfully ", tournament);

                return new GenericMessage { Message = Constants.APIErrorMessages.RECORD_UPDATED };

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        /// <summary>
        /// all tournament dropdown response
        /// </summary>
        /// <param name="email">user email who is requesting</param>
        /// <param name="userId">user email who is requesting</param>
        /// <returns></returns>
        public async Task<ErrorOr<List<GenericObj>>> GetAllTournamentDropdownAsync(string email, long userId, long activityCategoryId)
        {
            try
            {
                var tournament = await _itournamentRepository.GetAll(activityCategoryId).ToListAsync();
                List<GenericObj> genericObjList = tournament.Adapt<List<GenericObj>>();

                return genericObjList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        /// <summary>
        /// pick top teams from season of the tournament and create events for them
        /// </summary>
        /// <param name="email">user email who is requesting</param>
        /// <param name="userId">user id who is requesting</param>
        /// <param name="tournamentId">tournament id to schedule playoffs</param>
        /// <returns></returns>
        public async Task<ErrorOr<string>> SchedulePlayoffMatchesAsync(string email, long userId, long tournamentId)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            TournamentMapper tournamentMapper = new TournamentMapper();
            List<MatchUpDetails> matchUps = new List<MatchUpDetails>();
            EventCollectionMapper eventCollectionMapper = new EventCollectionMapper();
            TournamentEventCollectionMapper tournamentEventCollectionMapper = new TournamentEventCollectionMapper();
            EventMapper eventMapper = new EventMapper();
            EventCollectionTeamMapper eventCollectionTeamMapper = new EventCollectionTeamMapper();
            EventTeamMapper eventTeamMapper = new EventTeamMapper();

            try
            {
                Domain.Entities.Tournament tournament = await _itournamentRepository.GetTournamentByIdAsync(tournamentId, userId);
                List<TournamentVenue> tournamentVenueList = await _itournamentVenueRepository.GetAllTournamentVenueAsync(tournamentId);
                List<AllStatkeepersVenueList> tournamentStatkeeperList = await _itournamentStatkeeperRepository.GetStatkeeprList(tournamentId);
                var seasonTopTeams = await _itournamentRepository.GetTournamentTopTeamsToSchedulePlayoffRound(tournamentId);
                int teamCount = seasonTopTeams.Count();

                if (tournament is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Tournament.NoTournamentFound}");
                    return Domain.Common.Errors.Errors.Tournament.NoTournamentFound;
                }
                else if (!tournamentStatkeeperList.Any())
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Tournament.NoStatkeeperFound}");
                    return Domain.Common.Errors.Errors.Tournament.NoStatkeeperFound;
                }
                else if (!tournamentVenueList.Any())
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Tournament.NotEnoughVenuesToRandomize}");
                    return Domain.Common.Errors.Errors.Tournament.NotEnoughVenuesToRandomize;
                }
                else if (teamCount != tournament.TeamCapacity)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Tournament.NotEnoughTeamsToRandomize}");
                    return Domain.Common.Errors.Errors.Tournament.NotEnoughTeamsToRandomize;
                }
                else
                {
                    DateTime startDate = tournament.StartDateTime;
                    DateTime endDate = tournament.EndDateTime;

                    List<int> matchesPattern = tournamentMapper.CalculateSeeding(teamCount);
                    matchUps = tournamentMapper.CreateMatchUpsForPlayOff(matchesPattern, seasonTopTeams);

                    var eventTypeList = await _iunitOfWork.Repository<Domain.Entities.EventType>().GetAllAsync();
                    var eventTypeId = eventTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventType.Regulation.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                    matchUps = tournamentMapper.AssignRandomStatkeeperAndVenueToMatchUps(startDate, endDate, matchUps, tournamentStatkeeperList);
                    List<MatchUpDetails> venueList = matchUps.DistinctBy(x => x.VenueId).ToList();

                    List<EventCollection> eventCollectionList = eventCollectionMapper.MapEventCollectionForCreate(venueList, tournament.ActivityId, eventTypeId, email, userId);

                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        eventCollectionList = await _ieventCollectionRepository.CreateEventCollectionList(eventCollectionList);

                        List<TournamentEventCollection> tournamentEventCollectionList = tournamentEventCollectionMapper.MapEventCollectionWithTournamentList(eventCollectionList, tournament.Id, email);
                        Domain.Entities.EventStatus eventStatus = await _ieventRepository.GetEventStatusByNameAsync(Domain.Enums.Enum.EventStatus.InActive.ToString());
                        tournamentEventCollectionList = await _itournamentEventCollectionRepository.CreateTournamentEventCollectionList(tournamentEventCollectionList);
                        int i = 0;
                        foreach (var match in matchUps)
                        {
                            var eventCollection = eventCollectionList.Where(x => x.VenueId == match.VenueId).FirstOrDefault();

                            List<EventCollectionTeam> eventCollectionTeamList = eventCollectionTeamMapper.MapEventCollectionTeamListForCreate(email, match.TeamList, eventCollection.Id);
                            eventCollectionTeamList = await _ieventCollectionTeamRepository.CreateEventCollectionTeamList(eventCollectionTeamList);

                            Domain.Entities.Event @event = eventMapper.MapEventForCreate(email, 0, match.EventDateTime);

                            @event = await _iunitOfWork.Repository<Domain.Entities.Event>().AddAsync(@event);
                            await _iunitOfWork.Commit(cancellationToken);

                            List<EventTeam> eventTeamList = eventTeamMapper.CreateEventTeamList(email, eventCollectionTeamList, @event.Id, eventStatus.Id,i);
                            eventTeamList = await _ieventRepository.CreateEventTeamList(eventTeamList);
                            i = i + 2;
                        }

                        dbContextTransaction.Commit();
                        return Constants.APIErrorMessages.MATCHES_RANDOMIZE_SUCESSFULLY;
                    }
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
