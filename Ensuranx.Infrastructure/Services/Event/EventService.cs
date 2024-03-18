using AutoMapper;
using Azure;
using ErrorOr;
using Ensuranx.Application.Contracts.Event;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Event;
using Ensuranx.Application.Requests.Venue;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Other;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Common.Errors;
using Ensuranx.Domain.Entities;
using Ensuranx.Domain.Enums;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ensuranx.Infrastructure.Services.Event
{
    /// <summary>
    /// actions related to event entity
    /// </summary>
    public class EventService : IEventService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;
        private readonly IEventCollectionTeamRepository _ieventCollectionTeamRepository;
        private readonly IEventRepository _ieventRepository;
        private readonly IEventTeamRepository _ieventTeamRepository;
        private readonly IStatRepository _istatRepository;
        private readonly ITeamMemberRepository _iteamMemberRepository;
        private readonly IEventLogRepository _ieventLogRepository;
        private readonly IEventTeamMemberStatResultRepository _ieventTeamMemberStatResultRepository;
        private readonly IImagesRepository _iimagesRepository;
        private readonly IDeviceRepository _ideviceRepository;

        private readonly ApplicationDbContext _context;

        public EventService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _ieventCollectionTeamRepository = new EventCollectionTeamRepository(context);
            _ieventRepository = new EventRepository(context);
            _ieventTeamRepository = new EventTeamRepository(context);
            _istatRepository = new StatRepository(context);
            _iteamMemberRepository = new TeamMemberRepository(context);
            _ieventLogRepository = new EventLogRepository(context);
            _ieventTeamMemberStatResultRepository = new EventTeamMemberStatResultRepository(context);
            _iimagesRepository = new ImagesRepository(context);
            _ideviceRepository = new DeviceRepository(context);
            _context = context;
        }

        /// <summary>
        /// creates event
        /// </summary>
        /// <param name="userId">user id who is creating an event</param>
        /// <param name="email">user email who is creating an event</param>
        /// <returns>returns success message or returns error problem object</returns>
        public async Task<ErrorOr<EventResponse>> CreateEventAsync(long userId, string email, CreateEvent createEvent)
        {
            EventMapper eventMapper = new EventMapper();
            CancellationToken cancellationToken = CancellationToken.None;
            EventDurationModel eventDurationModel = new EventDurationModel();

            try
            {
                List<EventCollectionTeam> eventCollectionTeamList = await _ieventCollectionTeamRepository.GetEventCollectionTeamList(createEvent);

                if (!eventCollectionTeamList.Any())
                {
                    _logger.LogWarning($"{Errors.Event.TeamNotFound}");
                    return Errors.Event.TeamNotFound;
                }
                else if (Ensuranx.Application.StaticFunction.Function.ContainsDuplicates(createEvent.teamIdList))
                {
                    _logger.LogWarning($"{Errors.Event.ErrorSameTeam}");
                    return Errors.Event.ErrorSameTeam;
                }
                else
                {
                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        long eventCollectionTeamId = eventCollectionTeamList.FirstOrDefault().Id;
                        EventTeam eventTeamStatusCheck = await _ieventTeamRepository.GetEventTeamByEventCollectionTeamIdAsync(eventCollectionTeamId);

                        if (eventTeamStatusCheck is not null && (
                            eventTeamStatusCheck.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Active.ToString().ToLower() ||
                            eventTeamStatusCheck.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.InActive.ToString().ToLower()
                            ))
                        {
                            List<EventLog> eventLogList = await _ieventLogRepository.GetEventLogListByEventId(eventTeamStatusCheck.EventId, userId);
                            var eventStartDateTime = eventTeamStatusCheck.Event.EventDateTime;
                            var eventDurationInMinutes = eventTeamStatusCheck.EventCollectionTeam.EventCollection.Activity.MinutesOfActivity.Value;
                            var isTimeBased = eventTeamStatusCheck.EventCollectionTeam.EventCollection.Activity.ActivityCategory.IsTimeBased;
                            var eventEndDateTime = eventTeamStatusCheck.Event.EventEndDateTime;
                            var defaultDateTime = new DateTime();
                            bool isEventEnd = eventEndDateTime != defaultDateTime ? true : false;
                            bool isEventStart = eventTeamStatusCheck.Event.IsEventStart;

                            if (!eventTeamStatusCheck.Event.IsEventStart)
                            {
                                eventDurationModel.eventDurationMinute = eventDurationInMinutes;
                                eventDurationModel.eventDurationSecond = 0;
                            }
                            else
                            {
                                eventDurationModel = eventMapper.CalculateEventPausedTimeDurationInMinutes(eventLogList, eventStartDateTime, eventDurationInMinutes, isEventEnd, eventEndDateTime, isEventStart);
                            }

                            double shotClockTimer = 0;
                            bool isEventPause = false;

                            var lastShotClockTime = eventLogList.Where(x => x.IsShotClock != false).LastOrDefault();
                            if (lastShotClockTime is not null)
                            {
                                var totalSecondsPassedSinceLastShotClock = Math.Round(DateTime.UtcNow.Subtract(lastShotClockTime.CreatedDateTime).TotalSeconds);

                                shotClockTimer = totalSecondsPassedSinceLastShotClock <= Constants.APIErrorMessages.SHOT_CLOCK_TIMER ? Constants.APIErrorMessages.SHOT_CLOCK_TIMER - totalSecondsPassedSinceLastShotClock : 0;
                            }

                            var checkIfTheGameIsPaused = eventLogList.LastOrDefault();
                            isEventPause = checkIfTheGameIsPaused is not null && checkIfTheGameIsPaused.IsPause == 1 ? true : false;

                            _logger.LogInformation($"EventId = {eventTeamStatusCheck.EventId} InProgress = {true} Message = {Constants.APIErrorMessages.EVENT_INPROGRESS}");

                            return new EventResponse
                            {
                                EventId = eventTeamStatusCheck.EventId,
                                InProgress = !eventTeamStatusCheck.Event.IsEventStart ? false : true,
                                EventDurationInMinutes = eventDurationModel.eventDurationMinute,
                                EventDurationInSeconds = eventDurationModel.eventDurationSecond,
                                TotalEventDurationInMinutes = eventDurationInMinutes,
                                IsTimeBased = isTimeBased,
                                ShotClockTimer = shotClockTimer,
                                IsEventPause = isEventPause,
                                Message = Constants.APIErrorMessages.EVENT_INPROGRESS
                            };
                        }
                        else
                        {
                            List<long> teamIdList = eventCollectionTeamList.Select(x => x.TeamId).ToList();
                            List<TeamMember> teamMemberList = await _iteamMemberRepository.GetTeamMembersList(userId, teamIdList);
                            double shotClockTimer = 0;
                            bool isEventPause = false;
                            List<long?> check = teamMemberList.Select(x => x.PlayerId).Distinct().ToList();

                            if (teamMemberList.Count == check.Count)
                            {
                                Domain.Entities.Event @event = eventMapper.MapEventForCreate(email, userId, DateTime.UtcNow);
                                await _iunitOfWork.Repository<Domain.Entities.Event>().AddAsync(@event);
                                await _iunitOfWork.Commit(cancellationToken);

                                _logger.LogInformation("Event created successfully {@event}", @event);

                                EventStatus eventStatus = await _ieventRepository.GetEventStatusByNameAsync(Domain.Enums.Enum.EventStatus.Active.ToString());
                                List<EventTeam> eventTeamList = eventMapper.MapEventTeamForCreate(email, @event.Id, eventCollectionTeamList, eventStatus);

                                eventTeamList = await _ieventRepository.CreateEventTeamList(eventTeamList);
                                _logger.LogInformation("eventTeamList created successfully {@eventTeamList}", eventTeamList);

                                var eventDurationInMinutes = eventCollectionTeamList.FirstOrDefault().EventCollection.Activity.MinutesOfActivity.Value;
                                var isTimeBased = eventCollectionTeamList.FirstOrDefault().EventCollection.Activity.ActivityCategory.IsTimeBased;

                                //checking if the teamMember are substituted before delete those players
                                var teamMemberListToDelete = teamMemberList.Where(x => x.IsSubstituteOrDisqualify != false).ToList();
                                teamMemberListToDelete = await _iteamMemberRepository.DeleteTeamMemberList(teamMemberListToDelete);

                                _logger.LogInformation("teamMemberList for substitute player deleted successfully {@teamMemberListToDelete}", teamMemberListToDelete);


                                dbContextTransaction.Commit();

                                _logger.LogInformation($"EventId = {@event.Id} InProgress = {false} Message = {Constants.APIErrorMessages.EVENT_CREATED}");
                                return new EventResponse
                                {
                                    EventId = @event.Id,
                                    InProgress = false,
                                    EventDurationInMinutes = eventDurationInMinutes,
                                    EventDurationInSeconds = 0,
                                    TotalEventDurationInMinutes = eventDurationInMinutes,
                                    IsTimeBased = isTimeBased,
                                    IsEventPause = isEventPause,
                                    ShotClockTimer = shotClockTimer,
                                    Message = Constants.APIErrorMessages.EVENT_CREATED
                                };
                            }
                            else
                            {
                                return Errors.Event.DuplicateTeamMember;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            }
        }

        /// <summary>
        /// update the event start date time
        /// </summary>
        /// <param name="userId">user who is requesting this api</param>
        /// <param name="email">user who is requesting this api</param>
        /// <param name="eventId">event/match which is starting</param>
        /// <returns>problem statement in case of error and message model in case of success</returns>
        public async Task<ErrorOr<GenericMessage>> StartEventAsync(long userId, string email, long eventId)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            EventTeamMapper eventTeamMapper = new EventTeamMapper();

            try
            {
                Domain.Entities.Event @event = await _iunitOfWork.Repository<Domain.Entities.Event>().GetByIdAsync(eventId);
                List<EventTeam> eventTeamList = await _ieventTeamRepository.GetEventTeamByEventIdAsync(eventId);


                if (@event is null)
                {
                    _logger.LogWarning($"{Errors.Event.EventNotFound}");
                    return Errors.Event.EventNotFound;
                }
                if (!eventTeamList.Any())
                {
                    _logger.LogWarning($"{Errors.Event.TeamNotFound}");
                    return Errors.Event.TeamNotFound;
                }
                else
                {
                    @event.EventDateTime = DateTime.UtcNow;
                    @event.IsEventStart = true;

                    List<Domain.Entities.EventStatus> eventStatusList = await _iunitOfWork.Repository<Domain.Entities.EventStatus>().GetAllAsync();

                    var activeEventStatus = eventStatusList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventStatus.Active.ToString().ToLower()).FirstOrDefault();

                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        await _iunitOfWork.Repository<Domain.Entities.Event>().UpdateAsync(@event);
                        await _iunitOfWork.Commit(cancellationToken);

                        eventTeamList = eventTeamMapper.MapEventTeamStatus(eventTeamList, activeEventStatus);

                        eventTeamList = await _ieventTeamRepository.UpdateEventTeamAsync(eventTeamList);

                        dbContextTransaction.Commit();
                        _logger.LogInformation($"{Constants.APIErrorMessages.EVENT_STARTED_SUCCESSFULLY}");
                        return new GenericMessage { Message = Constants.APIErrorMessages.EVENT_STARTED_SUCCESSFULLY };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Errors.Authentication.ExceptionMessage;
            }
        }

        /// <summary>
        /// get scoreboard of event with teams
        /// </summary>
        /// <param name="eventId">match or event id</param>
        /// <returns>problem detail in case of error or teams,players and scoreboard in case of success</returns>
        public async Task<ErrorOr<EventScoreboad>> GetEventScoreboardAsync(long eventId, long userId, string email)
        {
            EventMapper eventMapper = new EventMapper();

            try
            {
                Domain.Entities.Event @event = await _iunitOfWork.Repository<Domain.Entities.Event>().GetByIdAsync(eventId);

                if (@event is null)
                {
                    return Domain.Common.Errors.Errors.Event.EventNotFound;
                }
                else
                {
                    List<EventTeam> eventTeamList = await _ieventTeamRepository.GetEventTeamByEventIdAsync(eventId);
                    long eventCollectionId = eventTeamList.FirstOrDefault().EventCollectionTeam.EventCollectionId;
                    List<EventLog> eventLogList = await _ieventLogRepository.GetEventLogListByEventId(eventId, userId);
                    List<long> teamIdList = eventTeamList.Select(x => x.EventCollectionTeam.Team.Id).ToList();
                    long activityId = eventTeamList.First().EventCollectionTeam.EventCollection.ActivityId;
                    List<Stat> statList = await _istatRepository.GetActivityStatListAsync(activityId);

                    List<TeamMember> teamMemberList = await _iteamMemberRepository.GetTeamMembersDeletedList(userId, teamIdList);
                    List<long> teamPlayersIdsList = teamMemberList.Select(x => x.PlayerId.Value).ToList();
                    List<WaitingList> waitingList = await _ideviceRepository.ShiftThesePlayerToWaitingListAsync(eventCollectionId, teamPlayersIdsList);
                    EventScoreboad eventScoreboad = eventMapper.MapEventScoreboard(eventTeamList, statList, teamMemberList, eventLogList, waitingList);

                    _logger.LogInformation("Event Scoreboard {@eventScoreboad}", eventScoreboad);
                    return eventScoreboad;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            }
        }

        /// <summary>
        /// Update Score of the match/event
        /// </summary>
        /// <param name="updateEventScoreboard">contains event id, team member id, stat id,is successfull or not</param>
        /// <param name="userId">user id who is updating an event</param>
        /// <param name="email">user email who is updating an event</param>
        /// <returns>problem statement in case of error and success message in case of success</returns>
        public async Task<ErrorOr<UpdateEventScoreboardResponse>> UpdateEventScoreboardAsync(UpdateEventScoreboard updateEventScoreboard, long userId, string email)
        {
            EventMapper eventMapper = new EventMapper();
            CancellationToken cancellationToken = CancellationToken.None;
            EventLog eventLog = new EventLog();

            try
            {
                if (updateEventScoreboard.IsShotClock != false)
                {
                    eventLog = eventMapper.MapEventLog(email, updateEventScoreboard);

                    await _iunitOfWork.Repository<Domain.Entities.EventLog>().AddAsync(eventLog);
                    await _iunitOfWork.Commit(cancellationToken);
                }
                else if (updateEventScoreboard.IsPause != 0)
                {
                    eventLog = eventMapper.MapEventLog(email, updateEventScoreboard);

                    await _iunitOfWork.Repository<Domain.Entities.EventLog>().AddAsync(eventLog);
                    await _iunitOfWork.Commit(cancellationToken);
                }
                else if (updateEventScoreboard.IsFix != true)
                {
                    eventLog = eventMapper.MapEventLog(email, updateEventScoreboard);

                    await _iunitOfWork.Repository<Domain.Entities.EventLog>().AddAsync(eventLog);
                    await _iunitOfWork.Commit(cancellationToken);
                }
                else
                {
                    eventLog = await _ieventLogRepository.GetEventLogByEventMemberStatId(updateEventScoreboard.EventId, updateEventScoreboard.TeamMemberId, updateEventScoreboard.StatId, updateEventScoreboard.IsSucessfull);
                    eventLog.IsFix = true;
                    eventLog.IsSucessfull = true;
                    eventLog.LastModifiedDateTime = DateTime.UtcNow;
                    await _iunitOfWork.Repository<Domain.Entities.EventLog>().UpdateAsync(eventLog);
                    await _iunitOfWork.Commit(cancellationToken);
                }

                _logger.LogInformation("Event log created successfully {@eventLog}", eventLog);
                return new UpdateEventScoreboardResponse { Message = Constants.APIErrorMessages.SCORE_UPDATED_SUCESSFULLY };
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            }
        }

        /// <summary>
        /// End Event/Match date
        /// </summary>
        /// <param name="eventId">event id</param>
        /// <param name="userId">user who is updating</param>
        /// <param name="email">user who is updating</param>
        /// <returns>problem detail in case of error and string success message in case of sucess</returns>
        public async Task<ErrorOr<EndEventResponse>> EndEventAsync(long eventId, long userId, string email)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            EventMapper eventMapper = new EventMapper();
            try
            {
                Domain.Entities.Event @event = await _iunitOfWork.Repository<Domain.Entities.Event>().GetByIdAsync(eventId);

                if (@event is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Event.EventNotFound}");
                    return Domain.Common.Errors.Errors.Event.EventNotFound;
                }
                else
                {
                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        @event.EventEndDateTime = DateTime.UtcNow;
                        await _iunitOfWork.Repository<Domain.Entities.Event>().UpdateAsync(@event);
                        await _iunitOfWork.Commit(cancellationToken);

                        List<Domain.Entities.EventTeam> eventTeamList = await _ieventTeamRepository.GetEventTeamByEventIdAsync(eventId);
                        List<Domain.Entities.EventLog> eventLogList = await _ieventLogRepository.GetEventLogListByEventId(eventId, userId);
                        List<long> teamIdList = eventTeamList.Select(e => e.EventCollectionTeam.TeamId).ToList();
                        List<Domain.Entities.TeamMember> teamMemberList = await _iteamMemberRepository.GetTeamMembersList(userId, teamIdList);
                        List<long> teamMemberPlayerIdList = teamMemberList.Select(x => x.PlayerId.Value).ToList();


                        List<Domain.Entities.EventStatus> eventStatusList = await _iunitOfWork.Repository<Domain.Entities.EventStatus>().GetAllAsync();

                        eventTeamList = eventMapper.UpdateEventTeamPoint(eventLogList, eventTeamList, teamMemberList, eventStatusList);
                        eventTeamList = await _ieventTeamRepository.UpdateEventTeamAsync(eventTeamList);

                        List<EventTeamMemberStatResult> eventTeamMemberStatResultList = eventMapper.MapEventTeamMemberStatResult(eventLogList, teamMemberList, eventTeamList,
                                                                                        email);

                        //finding Mvp Of the event
                        var mvpOfEvent = eventTeamMemberStatResultList.MaxBy(x => x.TotalScore);
                        mvpOfEvent.IsMvp = true;

                        eventTeamMemberStatResultList = await _ieventTeamMemberStatResultRepository.CreateRange(eventTeamMemberStatResultList);
                        _logger.LogInformation("eventTeamMemberStatResultList created successfully {@eventTeamMemberStatResultList}", eventTeamMemberStatResultList);

                        await _iunitOfWork.Repository<Domain.Entities.EventTeamMemberStatResult>().UpdateAsync(mvpOfEvent);
                        await _iunitOfWork.Commit(cancellationToken);

                        List<TeamMemberMatchesWin> numberOfGamesWon = await _ieventTeamRepository.GetEventTeamWinByTeamMemberIdList(teamMemberPlayerIdList);
                        List<EventTeamMemberStatResult> previousRecordOfTeamMembers = await _ieventTeamMemberStatResultRepository.GetTeamMembersPreviousDataAsync(teamMemberPlayerIdList);

                        eventTeamMemberStatResultList = eventMapper.CalculatePercentageForEvent(eventTeamMemberStatResultList, previousRecordOfTeamMembers, numberOfGamesWon);
                        eventTeamMemberStatResultList = await _ieventTeamMemberStatResultRepository.UpdateRange(eventTeamMemberStatResultList);
                        _logger.LogInformation("eventTeamMemberStatResultList updated successfully {@eventTeamMemberStatResultList}", eventTeamMemberStatResultList);

                        //get the loser team members to add them in waiting list
                        var teamId = eventTeamList.Where(x => x.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower()).Select(x => x.EventCollectionTeam.TeamId).FirstOrDefault();
                        var eventTypeCheck = eventTeamList.Select(x => x.EventCollectionTeam.EventCollection.EventType).FirstOrDefault();
                        if (teamId is not 0 && eventTypeCheck.Name.ToLower() != Domain.Enums.Enum.EventType.Regulation.ToString().ToLower())
                        {
                            List<long> teamPlayerIdList = teamMemberList.Where(x => x.TeamId == teamId).Select(x => x.PlayerId.Value).ToList();

                            long eventCollectionId = eventTeamList.Select(x => x.EventCollectionTeam.EventCollectionId).FirstOrDefault();
                            string eventTypeName = eventTeamList.Select(x => x.EventCollectionTeam.EventCollection.EventType.Name).FirstOrDefault();
                            await ShiftThesePlayerToWaitingList(eventCollectionId, teamPlayerIdList, eventTypeName, email);

                            //var teamToDeleted = eventTeamList.Where(x => x.EventCollectionTeam.TeamId == teamId).Select(x => x.EventCollectionTeam.Team).FirstOrDefault();

                            //teamToDeleted.IsDeleted = true;
                            //teamToDeleted.IsActive = false;

                            //await _iunitOfWork.Repository<Domain.Entities.Team>().UpdateAsync(teamToDeleted);
                            //await _iunitOfWork.Commit(cancellationToken);
                            //_logger.LogInformation("loser team gets deleted successfully {@teamToDeleted}", teamToDeleted);
                        }

                        dbContextTransaction.Commit();
                        _logger.LogInformation($"{Constants.APIErrorMessages.EVENT_UPDATED}");
                        return new EndEventResponse { Message = Constants.APIErrorMessages.EVENT_UPDATED };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            }
        }

        /// <summary>
        /// shift or add players to the waitingList
        /// </summary>
        /// <param name="eventCollectionId">collection/lobby id</param>
        /// <param name="userInfoIdList">user who is accessing the endpoint</param>
        /// <param name="eventTypeName">user who is accessing the endpoint</param>
        /// <returns></returns>
        public async Task<bool> ShiftThesePlayerToWaitingList(long eventCollectionId, List<long> userInfoIdList, string eventTypeName, string email)
        {
            DeviceMapper deviceMapper = new DeviceMapper();
            List<WaitingList> playerToShiftList = await _ideviceRepository.ShiftThesePlayerToWaitingListAsync(eventCollectionId, userInfoIdList);

            if (playerToShiftList.Any())
            {
                foreach (WaitingList waitList in playerToShiftList)
                {
                    waitList.IsDeleted = false;
                    waitList.IsActive = true;
                    waitList.LastModifiedDateTime = DateTime.UtcNow;
                }

                await _ideviceRepository.UpdateWaitingListAsync(playerToShiftList);
                _logger.LogInformation("Waiting list updated successfully {@playerToShiftList}", playerToShiftList);
            }

            if (userInfoIdList.Count != playerToShiftList.Count)
            {
                List<long> playerListToAddToWaitingList = userInfoIdList.Where(x => !playerToShiftList.Select(x => x.UserInfoId).Contains(x)).ToList();
                List<WaitingList> createNewWaitingListMembers = deviceMapper.CreateNewWaitingListPlayerList(playerListToAddToWaitingList, eventCollectionId, email);

                await _ideviceRepository.CreateWaitingListAsync(createNewWaitingListMembers);
                _logger.LogInformation("Waiting list created successfully {@createNewWaitingListMembers}", createNewWaitingListMembers);
            }

            return true;
        }

        /// <summary>
        /// Get the match or event results by event id
        /// </summary>
        /// <param name="eventId">result of the particular event id</param>
        /// <param name="userId">user id who is requesting the endpoint</param>
        /// <param name="email">user email who is requesting the endpoint</param>
        /// <returns>problem statement in case of error and incase of success returns the teams scorecard</returns>
        public async Task<ErrorOr<EventResultResponse>> GetEventResultAsync(long eventId, long userId, string email)
        {
            EventMapper eventMapper = new EventMapper();
            EventResultResponse eventResultResponse = new EventResultResponse();

            try
            {
                Domain.Entities.Event @event = await _iunitOfWork.Repository<Domain.Entities.Event>().GetByIdAsync(eventId);

                List<EventTeamMemberStatResult> eventTeamMemberStatResultList = await _ieventTeamMemberStatResultRepository.GetEventTeamMemberStatResultByEventIdAsync(eventId);

                if (@event is null)
                {
                    return Domain.Common.Errors.Errors.Event.EventNotFound;
                }
                else if (!eventTeamMemberStatResultList.Any())
                {
                    List<EventLog> eventLogList = await _ieventLogRepository.GetEventLogListByEventId(@event.Id, userId);

                    List<Domain.Entities.EventTeam> eventTeamList = await _ieventTeamRepository.GetEventTeamByEventIdAsync(eventId);
                    List<long> teamIdList = eventTeamList.Select(e => e.EventCollectionTeam.TeamId).ToList();
                    List<Domain.Entities.TeamMember> teamMemberList = await _iteamMemberRepository.GetTeamMembersAllPlayedList(userId, teamIdList);
                    long eventCollectionId = eventTeamList.FirstOrDefault().EventCollectionTeam.EventCollectionId;
                    long statkeeperId = eventTeamList.First().EventCollectionTeam.EventCollection.UserInfo.Id;
                    long activityId = eventTeamList.First().EventCollectionTeam.EventCollection.ActivityId;
                    Domain.Entities.Images imageStatkeeper = await _iimagesRepository.GetUserProfileImage(statkeeperId);
                    Domain.Entities.Images imageActivity = await _iimagesRepository.GetActivityImage(activityId);
                    List<long> teamPlayersIdsList = teamMemberList.Select(x => x.PlayerId.Value).ToList();
                    List<EventTeamMemberStatResult> previousRecordOfTeamMembers = await _ieventTeamMemberStatResultRepository.GetTeamMembersPreviousDataAsync(teamPlayersIdsList);
                    List<Domain.Entities.Images> memberImagesList = await _iimagesRepository.GetUserProfileImagesByUserIdList(teamPlayersIdsList);
                    List<EventPlayedByPlayerCount> eventPlayedByPlayerCountList = await _ieventTeamMemberStatResultRepository.GetEventTeamMemberStatResultByEventIdAsync(teamPlayersIdsList);
                    List<WaitingList> waitingList = await _ideviceRepository.ShiftThesePlayerToWaitingListAsync(eventCollectionId, teamPlayersIdsList);
                    List<TeamMemberMatchesWin> numberOfGamesWon = await _ieventTeamRepository.GetEventTeamWinByTeamMemberIdList(teamPlayersIdsList);
                    var eventDurationInMinutes = eventTeamList.First().EventCollectionTeam.EventCollection.Activity.MinutesOfActivity.Value;
                    var eventStartDateTime = eventTeamList.First().Event.EventDateTime;
                    var eventEndDateTime = eventTeamList.First().Event.EventEndDateTime;
                    var defaultDateTime = new DateTime();
                    bool isEventEnd = eventEndDateTime != defaultDateTime ? true : false;


                    EventDurationModel eventDurationModel = eventMapper.CalculateEventPausedTimeDurationInMinutes(eventLogList, eventStartDateTime, eventDurationInMinutes, isEventEnd, eventEndDateTime, @event.IsEventStart);

                    eventResultResponse = eventMapper.MapActiveEventResult(eventLogList, teamMemberList, eventTeamList, imageStatkeeper, imageActivity, memberImagesList, eventPlayedByPlayerCountList,
                        previousRecordOfTeamMembers, waitingList, numberOfGamesWon, eventDurationModel, eventDurationInMinutes);

                    return eventResultResponse;
                }
                else
                {
                    List<EventLog> eventLogList = await _ieventLogRepository.GetEventLogListByEventId(@event.Id, userId);
                    List<Domain.Entities.EventTeam> eventTeamList = await _ieventTeamRepository.GetEventTeamByEventIdAsync(eventId);
                    List<long> teamIdList = eventTeamList.Select(e => e.EventCollectionTeam.TeamId).ToList();
                    List<Domain.Entities.TeamMember> teamMemberList = await _iteamMemberRepository.GetTeamMembersAllList(userId, teamIdList);
                    long eventCollectionId = eventTeamList.FirstOrDefault().EventCollectionTeam.EventCollectionId;
                    long statkeeperId = eventTeamList.First().EventCollectionTeam.EventCollection.UserInfo.Id;
                    long activityId = eventTeamList.First().EventCollectionTeam.EventCollection.ActivityId;
                    Domain.Entities.Images imageStatkeeper = await _iimagesRepository.GetUserProfileImage(statkeeperId);
                    Domain.Entities.Images imageActivity = await _iimagesRepository.GetActivityImage(activityId);
                    List<long> teamPlayersIdsList = teamMemberList.Select(x => x.PlayerId.Value).ToList();
                    List<EventTeamMemberStatResult> previousRecordOfTeamMembers = await _ieventTeamMemberStatResultRepository.GetTeamMembersPreviousDataAsync(teamPlayersIdsList);
                    List<Domain.Entities.Images> memberImagesList = await _iimagesRepository.GetUserProfileImagesByUserIdList(teamPlayersIdsList);
                    List<EventPlayedByPlayerCount> eventPlayedByPlayerCountList = await _ieventTeamMemberStatResultRepository.GetEventTeamMemberStatResultByEventIdAsync(teamPlayersIdsList);
                    List<WaitingList> waitingList = await _ideviceRepository.ShiftThesePlayerToWaitingListAsync(eventCollectionId, teamPlayersIdsList);
                    var eventDurationInMinutes = eventTeamList.First().EventCollectionTeam.EventCollection.Activity.MinutesOfActivity.Value;
                    var eventStartDateTime = eventTeamList.First().Event.EventDateTime;
                    var eventEndDateTime = eventTeamList.First().Event.EventEndDateTime;
                    var defaultDateTime = new DateTime();
                    bool isEventEnd = eventEndDateTime != defaultDateTime ? true : false;

                    EventDurationModel eventDurationModel = eventMapper.CalculateEventPausedTimeDurationInMinutes(eventLogList, eventStartDateTime, eventDurationInMinutes, isEventEnd, eventEndDateTime, @event.IsEventStart);

                    eventResultResponse = eventMapper.MapEventResult(eventTeamMemberStatResultList, teamMemberList, eventTeamList, imageStatkeeper, imageActivity, memberImagesList, eventPlayedByPlayerCountList,
                        previousRecordOfTeamMembers, waitingList, eventDurationInMinutes, eventDurationModel);

                    return eventResultResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Authentication.ExceptionMessage;
            }
        }

        /// <summary>
        /// delete the eventCollection/Lobby
        /// </summary>
        /// <param name="eventCollectionId">the eventCollection</param>
        /// <param name="userId">user id who is requesting the endpoint</param>
        /// <param name="email">user email who is requesting the endpoint</param>
        /// <returns></returns>
        public async Task<ErrorOr<GenericMessage>> GetEndEventCollection(long eventCollectionId, long userId, string email)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {

                EventCollection DeleteEventCollection = await _context.EventCollection.FirstOrDefaultAsync(x => x.Id == eventCollectionId && x.StatKeeperId == userId);
                if (DeleteEventCollection != null)
                {
                    DeleteEventCollection.IsDeleted = true;
                    DeleteEventCollection.IsActive = false;

                    await _iunitOfWork.Repository<Domain.Entities.EventCollection>().UpdateAsync(DeleteEventCollection);
                    await _iunitOfWork.Commit(cancellationToken);

                    return new GenericMessage { Message = Constants.APIErrorMessages.DELETESUCCESSFULLY };
                }
                else
                {
                    return Domain.Common.Errors.Errors.Event.NoEventCollectionFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Event.NoEventCollectionFound;
            }
        }

        /// <summary>
        /// current status of the lobby/eventCollection
        /// </summary>
        /// <param name="eventCollectionId">the eventCollection</param>
        /// <param name="userId">user id who is requesting the endpoint</param>
        /// <param name="email">user email who is requesting the endpoint</param>
        /// <returns></returns>
        public async Task<ErrorOr<LockLobby>> EventCollectionLockStatusAsync(long eventCollectionId, long userId, string email)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {

                EventCollection EventCollection = await _iunitOfWork.Repository<Domain.Entities.EventCollection>().GetByIdAsync(eventCollectionId);

                if (EventCollection is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Event.NoEventCollectionFound}");
                    return Domain.Common.Errors.Errors.Event.NoEventCollectionFound;
                }
                else
                {
                    _logger.LogInformation($"{Constants.APIErrorMessages.LOBBY_STATUS_UPDATED} IsLock = {EventCollection.IsLocked}");
                    return new LockLobby { IsLock = EventCollection.IsLocked, Message = "" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Event.NoEventCollectionFound;
            }
        }

        public async Task<ErrorOr<MatchSummaryResp>> GetMatchSummary(BasicFilter paginationFilter, long eventId, long userId, long quaterId)
        {
            try
            {
                List<long> points = new List<long>();
                List<MatchSummaryRespList> SummaryDetail = new List<MatchSummaryRespList>();
                EventMapper eventMapper = new EventMapper();
                List<long> eventLogId; // Declare the variable and assign a default value
                var quarterTimes = new Dictionary<long, (DateTime start, DateTime end)>(); // Dictionary to store QuarterNumbers against their time

                var getStartEventDateTime = await _ieventTeamRepository.GetStartEventDateTime(eventId); // Get First Log time by eventId
                var getEndEventDateTime = await _ieventTeamRepository.GetEndEventDateTime(eventId); //Get last log time by eventId

                var ActivityTime = await _ieventRepository.GetActivityTime(eventId); // ActivityTime in minutes
                var Quarters = await _ieventRepository.GetQuaters(eventId);

                var ActivityQuarterTime = TimeSpan.FromMinutes(ActivityTime / Quarters); // Per Quater By mints

                DateTime quarterStartTime = getStartEventDateTime; // Start time of the first quarter

                for (int quarter = 0; quarter < Quarters; quarter++)
                {
                    DateTime quarterEndTime = quarterStartTime + ActivityQuarterTime; // Calculate the end time of the current quarter (6:51:301 + 12 mints)

                    quarterTimes.Add(quarter + 1, (quarterStartTime, quarterEndTime)); // Store the quarter times with their quarter values

                    quarterStartTime = quarterEndTime; // Update the start time for the next quarter 
                }

                if (quarterTimes.ContainsKey(quaterId))
                {
                    var quarterTime = quarterTimes[quaterId];
                    var startDateTime = quarterTime.start;
                    var endDateTime = quarterTime.end;

                    // Pass the start and end DateTime values to the getEventlogIds method
                    eventLogId = await _ieventTeamRepository.getEventlogIds(eventId, startDateTime, endDateTime);
                }
                else
                {
                    eventLogId = await _ieventTeamRepository.getEventlogIds(eventId, getStartEventDateTime, getEndEventDateTime);
                }

                if (eventLogId.Any())
                {
                    long teamOneScore = 0;
                    long teamTwoScore = 0;
                    var teamInEvents = _ieventTeamRepository.getEventTeam(eventId);
                    var teamOne = teamInEvents.Result.FirstOrDefault();


                    var details = await _ieventTeamRepository.getEventlog(paginationFilter, eventLogId);


                    foreach (var i in details)
                    {

                        var statId = i.statid;
                        var teamMemberId = i.TeamMemberId;
                        var teamIdinDetail = i.TeamId;

                        if (teamOne != null && teamIdinDetail != null)
                        {
                            if (teamIdinDetail == teamOne.TeamId)
                            {
                                teamOneScore += i.score;
                            }
                            else
                            {
                                teamTwoScore += i.score;
                            }
                            var getMatchSummaryLists = await _ieventTeamRepository.getMatchSummary(eventId, userId, teamOneScore, teamTwoScore, statId, teamMemberId);
                            SummaryDetail.Add(getMatchSummaryLists);
                        }
                    }
                }


                var getMatchSummary = eventMapper.MapMatchSummary(paginationFilter, getStartEventDateTime, SummaryDetail, Quarters);

                return getMatchSummary;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occurred {@ex}", ex);
                return Domain.Common.Errors.Errors.Event.NoEventCollectionFound;
            }
        }
    }
}
