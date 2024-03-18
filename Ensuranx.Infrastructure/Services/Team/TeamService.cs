using ErrorOr;
using FirebaseAdmin.Auth;
using Ensuranx.Application.Contracts.Team;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Team;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Team;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Common.Constant;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Mappers;
using Ensuranx.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using static Ensuranx.Domain.Common.Errors.Errors;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Services.Team
{
    /// <summary>
    /// Contains all the business logic for teams setup 
    /// </summary>
    public class TeamService : ITeamService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly ITeamMemberRoleRepository _iteamMemberRoleRepository;
        private readonly ITeamRepository _iteamRepository;
        private readonly IEventCollectionRepository _ieventCollectionRepository;
        private readonly ITeamMemberRepository _iteamMemberRepository;
        private readonly IUserInfoRepository _iuserInfoRepository;
        private readonly IImagesRepository _iimagesRepository;
        private readonly ITeamActivityRepository _iteamActivityRepository;
        private readonly IDeviceRepository _ideviceReposiroty;
        private readonly IActivityRepository _iactivityRepository;
        private readonly IEventCollectionTeamRepository _ieventCollectionTeamRepository;
        private readonly IFollowRepository _ifollowRepository;
        private readonly IEventTeamRepository _ieventTeamRepository;

        public TeamService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
            _context = context;
            _iteamMemberRoleRepository = new TeamMemberRoleRepository(context);
            _iteamRepository = new TeamRepository(context);
            _ieventCollectionRepository = new EventCollectionRepository(context);
            _iteamMemberRepository = new TeamMemberRepository(context);
            _iuserInfoRepository = new UserInfoRepository(context);
            _iimagesRepository = new ImagesRepository(context);
            _iteamActivityRepository = new TeamActivityRepository(_context);
            _ideviceReposiroty = new DeviceRepository(_context);
            _iactivityRepository = new ActivityRepository(_context);
            _ieventCollectionTeamRepository = new EventCollectionTeamRepository(_context);
            _ifollowRepository = new FollowRepository(_context);
            _ieventTeamRepository = new EventTeamRepository(_context);
        }

        string toBase26(long i)
        {
            if (i == 0) return ""; i--;
            return toBase26(i / 26) + (char)('A' + i % 26);
        }

        /// <summary>
        /// Create Temp Team And Add Member who create that team as memeber as captain
        /// </summary>
        /// <param name="userId">player who creates team</param>
        /// <param name="email">players email</param>
        /// <param name="eventCollectionId">this team is associated with eventCollectionId lobby</param>
        /// <returns>return model with success message and incase of error returns problem</returns>
        public async Task<ErrorOr<CreateTemporaryTeamResponse>> CreateTemporaryTeamAsync(long userId, string email, long eventCollectionId, long playerId)
        {
            TeamMapper teamMapper = new TeamMapper();
            WaitingListMapper waitingListMapper = new WaitingListMapper();
            EventCollectionTeamMapper eventCollectionTeamMapper = new EventCollectionTeamMapper();
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                EventCollection eventCollection = await _ieventCollectionRepository.GetEventCollectionByEventCollectionId(eventCollectionId);
                EventTeam eventTeamStatusCheckForActiveEvent = await _ieventTeamRepository.GetEventTeamByEventCollectionIdAndPlayerIdAsync(eventCollectionId, playerId, Domain.Enums.Enum.EventStatus.Active.ToString().ToLower());

                if (eventCollection is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.NoLobbyFound}");
                    return Domain.Common.Errors.Errors.Team.NoLobbyFound;
                }
                else if (eventCollection.EventType.Name.ToLower() != Domain.Enums.Enum.EventType.Pickup.ToString().ToLower())
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.UnAuthorizedToCreateTeam}");
                    return Domain.Common.Errors.Errors.Team.UnAuthorizedToCreateTeam;
                }
                else if (eventTeamStatusCheckForActiveEvent is not null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.PartOfActiveEvent}");
                    return Domain.Common.Errors.Errors.Team.PartOfActiveEvent;
                }
                else
                {
                    int randomGuid = createGuId();

                    var teamTypeList = await _iunitOfWork.Repository<Domain.Entities.TeamType>().GetAllAsync();
                    var teamTypeId = teamTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.TeamType.Temporary.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                    int teamCountInLobby = await _iteamRepository.GetTeamCountInEventCollection(eventCollection.Id);
                    teamCountInLobby = teamCountInLobby + 1;

                    string teamChar = toBase26(teamCountInLobby);

                    string teamName = Constants.APIErrorMessages.TEAM + teamChar;
                    Domain.Entities.Team team = teamMapper.MapTeam(teamTypeId, teamName, email);

                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        team = await CreateTeamAsync(team);

                        _logger.LogInformation("Team is created successfully {@team}", team);

                        EventCollectionTeam eventCollectionTeam = eventCollectionTeamMapper.MapEventCollectionTeamForCreate(email, team.Id, eventCollection.Id);

                        eventCollectionTeam = await _iunitOfWork.Repository<Domain.Entities.EventCollectionTeam>().AddAsync(eventCollectionTeam);
                        await _iunitOfWork.Commit(cancellationToken);
                        _logger.LogInformation("EventCollectionTeam is created successfully {@eventCollectionTeam}", eventCollectionTeam);

                        if (playerId != 0)
                        {
                            List<long> teamIdList = await _ieventCollectionTeamRepository.GetTeamIdListForAllTeamInTheSameTeamEventCollection(team.Id);

                            TeamMember teamMemberToRemove = await _iteamMemberRepository.CheckIfThisPlayerIsPresentInWhichTeamList(playerId, teamIdList);

                            if (teamMemberToRemove is not null)
                            {
                                long teamIdForMemberRoleUpdate = teamMemberToRemove.TeamId;

                                await _iunitOfWork.Repository<Domain.Entities.TeamMember>().DeleteAsync(teamMemberToRemove);
                                await _iunitOfWork.Commit(cancellationToken);

                                _logger.LogInformation($"player remove from previous team successfully");

                                //updating next member in the team as captain
                                TeamMember teamMemberToMakeCaptain = await _iteamMemberRepository.GetTeamMemberByTeamIdFirstOrDefault(teamIdForMemberRoleUpdate);
                                if (teamMemberToMakeCaptain is not null)
                                {
                                    Domain.Entities.TeamMemberRole captainRole = await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Captain.ToString());
                                    teamMemberToMakeCaptain.TeamMemberRoleId = captainRole.Id;

                                    await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(teamMemberToMakeCaptain);
                                    await _iunitOfWork.Commit(cancellationToken);

                                    _logger.LogInformation($"new captain assign successfully");
                                }

                                //check if this teamMember team have zero members
                                int countTeamMember = _iteamMemberRepository.GetTeamMemberCountByTeamId(teamIdForMemberRoleUpdate);

                                if (countTeamMember == 0)
                                {
                                    var deleteTeam = await _iunitOfWork.Repository<Domain.Entities.Team>().GetByIdAsync(teamIdForMemberRoleUpdate);
                                    if (deleteTeam is not null)
                                    {
                                        await _iteamRepository.DeleteTeamById(deleteTeam);
                                        _logger.LogInformation($"delete team successfully because team member count is {countTeamMember}");
                                    }
                                }
                            }

                            var teamMemberRoleId = await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Captain.ToString());
                            TeamMember teamMember = teamMapper.MapTeamMemberWithTeam(team.Id, email, teamMemberRoleId.Id, playerId);
                            teamMember.IsJoinedCollection = true;
                            teamMember = await _iunitOfWork.Repository<Domain.Entities.TeamMember>().AddAsync(teamMember);
                            await _iunitOfWork.Commit(cancellationToken);
                            _logger.LogInformation("TeamMember is created successfully {@teamMember}", teamMember);

                            var selectPlayerFromWaitingList = await _iteamMemberRoleRepository.GetWaitingListPlayer(playerId, eventCollectionId);

                            if (selectPlayerFromWaitingList is not null)
                            {
                                selectPlayerFromWaitingList.IsActive = false;
                                selectPlayerFromWaitingList.IsDeleted = true;

                                await _iunitOfWork.Repository<Domain.Entities.WaitingList>().UpdateAsync(selectPlayerFromWaitingList);
                                await _iunitOfWork.Commit(cancellationToken);
                            }
                            else
                            {
                                var lastWaitingListPlayer = await _ideviceReposiroty.GetLastPlayerNumberInTheList(eventCollectionId);
                                WaitingList waitingList = waitingListMapper.CreateWaitingListMap(email, playerId, eventCollectionId, lastWaitingListPlayer);
                                waitingList.IsActive = false;
                                waitingList.IsDeleted = true;

                                await _iunitOfWork.Repository<Domain.Entities.WaitingList>().AddAsync(waitingList);
                                await _iunitOfWork.Commit(cancellationToken);

                                _logger.LogInformation("player added to the waiting list in deleted state @{waitingList}", waitingList);
                            }
                        }

                        dbContextTransaction.Commit();

                        return new CreateTemporaryTeamResponse { TeamId = team.Id, Message = Constants.APIErrorMessages.TEAM_CREATED };
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
        /// contains logic for adding user to a perticular temp team
        /// </summary>
        /// <param name="userId">player who wants to join any team</param>
        /// <param name="email">email of that user</param>
        /// <param name="teamId">team which player wants to join</param>
        /// <returns>Incase of success return string message or incase of error returns problem</returns>
        public async Task<ErrorOr<AddPlayerToTeamResponse>> AddPlayerToTheTeamAsync(long userId, string email, long teamId, long playerId, long eventCollectionId)
        {
            TeamMapper teamMapper = new TeamMapper();
            WaitingListMapper waitingListMapper = new WaitingListMapper();
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                var isMember = await _iteamMemberRoleRepository.CheckMemberIsPresent(teamId, playerId);
                var teamLimit = _iteamMemberRepository.GetTeamMemberCountByTeamId(teamId);
                var maxTeamPlayerLimit = await _iactivityRepository.GetMaxTeamMemberCountByTeamId(teamId);
                var teamCheck = await _iteamRepository.GetTeamById(teamId);

                if (isMember is not null && isMember.IsActive != false && isMember.IsDeleted != true && isMember.IsSubstituteOrDisqualify != true && isMember.IsJoinedCollection != false)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.TeamMemberFound}");
                    return Domain.Common.Errors.Errors.Team.TeamMemberFound;
                }
                else if (isMember is null && teamCheck is not null && teamCheck.TeamType.Name.ToLower() != Domain.Enums.Enum.TeamType.Temporary.ToString().ToLower())
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.TeamMemberFound}");
                    return Domain.Common.Errors.Errors.Team.TeamMemberFound;
                }
                else if (teamId == 0)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.TeamIsRequired}");
                    return Domain.Common.Errors.Errors.Team.TeamIsRequired;
                }
                else if (!(teamLimit < maxTeamPlayerLimit))
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.TeamLimitExceeded}");
                    return Domain.Common.Errors.Errors.Team.TeamLimitExceeded;
                }
                else
                {
                    var teamMemberRoleId = teamLimit != 0 ?
                          await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Member.ToString())
                        : await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Captain.ToString());

                    List<long> teamIdList = await _ieventCollectionTeamRepository.GetTeamIdListForAllTeamInTheSameTeamEventCollection(teamId);
                    //long eventCollectionId = await _ieventCollectionTeamRepository.GetEventCollectionIdByTeamId(teamId);
                    TeamMember teamMemberToRemove = await _iteamMemberRepository.CheckIfThisPlayerIsPresentInWhichTeamList(playerId, teamIdList);
                    var checkMemberIsInWaitingList = await _ideviceReposiroty.GetWaitingListByPlayerId(playerId, eventCollectionId);

                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        if (checkMemberIsInWaitingList is null)
                        {
                            var lastWaitingListPlayer = await _ideviceReposiroty.GetLastPlayerNumberInTheList(eventCollectionId);
                            WaitingList waitingList = waitingListMapper.CreateWaitingListMap(email, playerId, eventCollectionId, lastWaitingListPlayer);

                            await _iunitOfWork.Repository<Domain.Entities.WaitingList>().AddAsync(waitingList);
                            await _iunitOfWork.Commit(cancellationToken);

                            _logger.LogInformation("player added to the waiting list @{waitingList}", waitingList);
                        }

                        if (teamMemberToRemove is not null)
                        {
                            long teamIdForMemberRoleUpdate = teamMemberToRemove.TeamId;

                            await _iunitOfWork.Repository<Domain.Entities.TeamMember>().DeleteAsync(teamMemberToRemove);
                            await _iunitOfWork.Commit(cancellationToken);

                            _logger.LogInformation($"player remove from previous team successfully");

                            //check if this teamMember team have zero members
                            int countTeamMember = _iteamMemberRepository.GetTeamMemberCountByTeamId(teamMemberToRemove.TeamId);

                            if (countTeamMember == 0)
                            {
                                var deleteTeam = await _iunitOfWork.Repository<Domain.Entities.Team>().GetByIdAsync(teamMemberToRemove.TeamId);
                                if (deleteTeam is not null)
                                {
                                    await _iteamRepository.DeleteTeamById(deleteTeam);
                                    _logger.LogInformation($"delete team successfully because team member count is {countTeamMember}");
                                }
                            }

                            //updating next member in the team as captain
                            TeamMember teamMemberToMakeCaptain = await _iteamMemberRepository.GetTeamMemberByTeamIdFirstOrDefault(teamIdForMemberRoleUpdate);

                            if (teamMemberToMakeCaptain is not null && teamMemberToMakeCaptain.Team.TeamType.Name.ToLower() != Domain.Enums.Enum.TeamType.Official.ToString().ToLower())
                            {
                                Domain.Entities.TeamMemberRole captainRole = await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Captain.ToString());
                                teamMemberToMakeCaptain.TeamMemberRoleId = captainRole.Id;

                                await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(teamMemberToMakeCaptain);
                                await _iunitOfWork.Commit(cancellationToken);

                                _logger.LogInformation($"new captain assign successfully");
                            }

                            bool checkRemove = await AssignSequenceToTheTeamRemove(eventCollectionId, teamMemberToRemove.TeamId);
                        }

                        if (isMember is not null)
                        {
                            isMember.IsActive = true;
                            isMember.IsDeleted = false;
                            isMember.IsJoinedCollection = true;
                            await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(isMember);
                            await _iunitOfWork.Commit(cancellationToken);
                        }
                        else
                        {
                            TeamMember teamMember = teamMapper.MapTeamMemberWithTeam(teamId, email, teamMemberRoleId.Id, playerId);
                            teamMember.IsJoinedCollection = true;
                            teamMember = await _iunitOfWork.Repository<Domain.Entities.TeamMember>().AddAsync(teamMember);
                            await _iunitOfWork.Commit(cancellationToken);
                        }


                        var selectPlayerFromWaitingList = await _iteamMemberRoleRepository.GetWaitingListPlayer(playerId, eventCollectionId);

                        if (selectPlayerFromWaitingList is not null)
                        {
                            selectPlayerFromWaitingList.IsActive = false;
                            selectPlayerFromWaitingList.IsDeleted = true;

                            await _iunitOfWork.Repository<Domain.Entities.WaitingList>().UpdateAsync(selectPlayerFromWaitingList);
                            await _iunitOfWork.Commit(cancellationToken);
                        }


                        // assign sequence to the team
                        bool check = await AssignSequenceToTheTeam(eventCollectionId, teamId);

                        dbContextTransaction.Commit();
                        _logger.LogInformation($"{Constants.APIErrorMessages.TEAM_UPDATED_SUCESSFULLY}");
                        return new AddPlayerToTeamResponse { Message = Constants.APIErrorMessages.TEAM_UPDATED_SUCESSFULLY };
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
        /// assign sequenece to the team
        /// </summary>
        /// <param name="eventCollectionId">lobby/eventCollection id</param>
        /// <param name="teamId">team which is full</param>
        /// <returns></returns>
        public async Task<bool> AssignSequenceToTheTeam(long eventCollectionId, long teamId)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            var teamLimit = _iteamMemberRepository.GetTeamMemberCountByTeamId(teamId);
            var eventCollection = await _ieventCollectionRepository.GetEventCollectionByEventCollectionId(eventCollectionId);

            if (eventCollection is null)
            {
                _logger.LogWarning("EventCollection not found");
                return false;
            }
            else
            {
                if (teamLimit == eventCollection.Activity.MaxPlayerPerTeam)
                {
                    string topSequence = await _ieventCollectionTeamRepository.GetMaxSequenceEventCollectionTeamByEventCollectionId(eventCollectionId);
                    topSequence = topSequence != "A" ? (Convert.ToInt32(topSequence) + 1).ToString() : "1";

                    EventCollectionTeam eventCollectionTeam = await _ieventCollectionTeamRepository.GetEventCollectionIdByTeamIdAndEventCollectionIdAsync(teamId, eventCollectionId);
                    eventCollectionTeam.Sequence = topSequence;

                    await _iunitOfWork.Repository<EventCollectionTeam>().UpdateAsync(eventCollectionTeam);
                    await _iunitOfWork.Commit(cancellationToken);

                    _logger.LogInformation("Sequence Updated Successfully {@eventCollectionTeam}", eventCollectionTeam);
                }

                return true;
            }
        }

        /// <summary>
        /// assign sequence to the team
        /// </summary>
        /// <param name="eventCollectionId">lobby/eventCollection id</param>
        /// <param name="teamId">team which is full</param>
        /// <returns></returns>
        public async Task<bool> AssignSequenceToTheTeamRemove(long eventCollectionId, long teamId)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            var teamLimit = _iteamMemberRepository.GetTeamMemberCountByTeamId(teamId);
            var eventCollection = await _ieventCollectionRepository.GetEventCollectionByEventCollectionId(eventCollectionId);

            if (eventCollection is null)
            {
                _logger.LogWarning("EventCollection not found");
                return false;
            }
            else
            {
                EventCollectionTeam eventCollectionTeam = await _ieventCollectionTeamRepository.GetEventCollectionIdByTeamIdAndEventCollectionIdAsync(teamId, eventCollectionId);
                string topSequence = await _ieventCollectionTeamRepository.GetMaxSequenceEventCollectionTeamByEventCollectionId(eventCollectionId);

                if (eventCollectionTeam.Sequence != "A")
                {
                    eventCollectionTeam.Sequence = "A";

                    await _iunitOfWork.Repository<EventCollectionTeam>().UpdateAsync(eventCollectionTeam);
                    await _iunitOfWork.Commit(cancellationToken);

                    _logger.LogInformation("Sequence Updated Successfully {@eventCollectionTeam}", eventCollectionTeam);
                }

                return true;
            }
        }

        /// <summary>
        /// create team in general
        /// </summary>
        /// <param name="team">Team Entity object</param>
        /// <returns>Created team object</returns>
        public async Task<Domain.Entities.Team> CreateTeamAsync(Domain.Entities.Team team)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            team = await _iunitOfWork.Repository<Domain.Entities.Team>().AddAsync(team);
            await _iunitOfWork.Commit(cancellationToken);
            return team;
        }

        /// <summary>
        /// update the team entity
        /// </summary>
        /// <param name="team">team entity</param>
        /// <returns>team entity</returns>
        public async Task<Domain.Entities.Team> UpdateTeam(Domain.Entities.Team team)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            await _iunitOfWork.Repository<Domain.Entities.Team>().UpdateAsync(team);
            await _iunitOfWork.Commit(cancellationToken);
            return team;
        }

        /// <summary>
        /// fetch temperory team based on venue and activity
        /// </summary>
        /// <param name="venueId">team which are playing in this venue</param>
        /// <param name="activityId">team which are playing in this activity</param>
        /// <param name="evenTypeId">team which are playing in this type of event</param>
        /// <param name="teamTypeId">team which are of this type</param>
        /// <returns>List of team and memebers with their roles</returns>
        public async Task<ErrorOr<PagedResponse<List<TeamMemberDetails>>>> GetAllTeams(long roleId, long userId, BasicFilter basicFilter)
        {
            _logger.LogInformation("GetAllTeams");
            List<TeamMemberDetails> teamMemberList = new List<TeamMemberDetails>();
            int totalRecords = 0;
            try
            {
                var role = await _iteamRepository.GetRole(roleId, userId);
                if (role == Domain.Enums.Enum.Roles.Player.ToString())
                {
                    teamMemberList = await _iteamRepository.GetTeamsWithMembersListByPlayerId(role, userId, basicFilter);
                    totalRecords = await _iteamRepository.GetTeamsWithMembersCountPlayer(userId, basicFilter.sSearch);
                    _logger.LogInformation("all Player response : {@teamMemberList}", teamMemberList);
                }
                else if (role == Domain.Enums.Enum.Roles.Business.ToString())
                {
                    teamMemberList = await _iteamRepository.GetTeamsWithMembersListByBusinessId(userId, basicFilter);
                    totalRecords = await _iteamRepository.GetTeamsWithMembersCount(userId);
                    _logger.LogInformation("all Business response : {@teamMemberList}", teamMemberList);
                    return new PagedResponse<List<TeamMemberDetails>>(totalRecords, teamMemberList, basicFilter.PageNumber, basicFilter.PageSize);
                }

                return new PagedResponse<List<TeamMemberDetails>>(totalRecords, teamMemberList, basicFilter.PageNumber, basicFilter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// recent teams for the overview screen
        /// </summary>
        /// <param name="userId">user who is accessing the endpoint</param>
        /// <param name="basicFilter">pagination filters</param>
        /// <returns>pagination response model</returns>
        public async Task<ErrorOr<PagedResponse<List<GetRecentTeam>>>> GetRecentTeamsAsync(long userId, BasicFilter basicFilter, string email)
        {
            _logger.LogInformation("GetRecentTeamsAsync");
            List<GetRecentTeam> getRecentTeamList = new List<GetRecentTeam>();
            int totalRecords = 0;

            try
            {
                getRecentTeamList = await _iteamRepository.GetRecentTeamsByPlayerIdAsync(userId, basicFilter);

                totalRecords = await _iteamRepository.GetRecentTeamsCountByPlayerIdAsync(userId);
                _logger.LogInformation("all Player response : {@getRecentTeamList}", getRecentTeamList);

                return new PagedResponse<List<GetRecentTeam>>(totalRecords, getRecentTeamList, basicFilter.PageNumber, basicFilter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// creates unique 6 digits
        /// </summary>
        /// <returns>6 digits unique integer</returns>
        public int createGuId()
        {
            _logger.LogInformation("Generating One-Time Password");
            Random rand = new Random((int)DateTime.Now.Ticks);
            int numIterations = rand.Next(100000, 999999);
            _logger.LogInformation($"One-Time Password is ready {numIterations}");
            return numIterations;
        }

        /// <summary>
        /// creating official teams
        /// </summary>
        /// <param name="userId">user id who is creating the offical team</param>
        /// <param name="email">user email who is creating the offical team</param>
        /// <param name="createTeam">contains team name and activity of the team</param>
        /// <returns></returns>
        public async Task<ErrorOr<CreateTeamResponse>> CreateOfficialTeamAsync(long userId, string email, CreateTeam createTeam)
        {
            TeamMapper teamMapper = new TeamMapper();
            CancellationToken cancellationToken = CancellationToken.None;


            try
            {
                var checkTeamNameTaken = await _iteamRepository.CheckTeamNameTaken(createTeam.Name);

                if (checkTeamNameTaken)
                {
                    return Domain.Common.Errors.Errors.Team.TeamNameIsTaken;
                }
                else
                {
                    var teamTypeList = await _iunitOfWork.Repository<Domain.Entities.TeamType>().GetAllAsync();
                    var teamTypeId = teamTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.TeamType.Official.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();
                    Domain.Entities.Team team = teamMapper.MapTeam(teamTypeId, createTeam.Name, email);

                    var eventTypeList = await _iunitOfWork.Repository<Domain.Entities.EventType>().GetAllAsync();
                    var eventTypeId = eventTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventType.Regulation.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        team = await CreateTeamAsync(team);
                        _logger.LogInformation("Team is created successfully {@team}", team);

                        Domain.Entities.Activity activity = await _iactivityRepository.GetActivityByEventTypeAndActivityCategoryId(eventTypeId, createTeam.ActivityId);

                        TeamActivity teamActivity = teamMapper.MapTeamActivity(team.Id, activity.Id, email);

                        teamActivity = await _iunitOfWork.Repository<Domain.Entities.TeamActivity>().AddAsync(teamActivity);
                        await _iunitOfWork.Commit(cancellationToken);
                        _logger.LogInformation("Team Activity is created successfully {@teamActivity}", teamActivity);

                        var teamMemberRoleId = await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Captain.ToString());
                        TeamMember teamMember = teamMapper.MapTeamMemberWithTeam(team.Id, email, teamMemberRoleId.Id, userId);

                        teamMember = await _iunitOfWork.Repository<Domain.Entities.TeamMember>().AddAsync(teamMember);
                        await _iunitOfWork.Commit(cancellationToken);
                        _logger.LogInformation("TeamMember is created successfully {@teamMember}", teamMember);

                        dbContextTransaction.Commit();

                        return new CreateTeamResponse { Message = Constants.APIErrorMessages.TEAM_CREATED };
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
        /// Remove or Ban player from the team 
        /// </summary>
        /// <param name="userId">user id who is requesting endpoint</param>
        /// <param name="email">user email who is requesting endpoint</param>
        /// <param name="removeOrBanPlayer">contains team and player id which needs to ban or remove checks</param>
        /// <returns>problem statement in case of error and in case of success returns string message</returns>
        public async Task<ErrorOr<RemoveOrBanPlayerResponse>> RemoveOrBanPlayerAsync(long userId, string email, RemoveOrBanPlayer removeOrBanPlayer)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {
                TeamMember isCaption = await _iteamMemberRepository.checkCaption(userId);

                if (isCaption.TeamMemberRoleId != ((int)Domain.Enums.Enum.TeamMemberRole.Captain))
                {
                    return Domain.Common.Errors.Errors.Team.PermissionDenied;
                }
                else
                {
                    TeamMember teamMember = await _iteamMemberRepository.GetTeamMemberRequestAcceptedByTeamIdAndPlayerId(removeOrBanPlayer.TeamId, removeOrBanPlayer.PlayerId);

                    if (teamMember is null)
                    {
                        _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.NoTeamMemberFound}");
                        return Domain.Common.Errors.Errors.Team.NoTeamMemberFound;
                    }
                    else
                    {
                        teamMember.IsBan = removeOrBanPlayer.IsBan;
                        if (removeOrBanPlayer.IsRemove)
                        {
                            teamMember.IsDeleted = true;
                            teamMember.IsActive = false;
                            teamMember.LeftDateTime = DateTime.UtcNow;
                        }
                        await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(teamMember);
                        await _iunitOfWork.Commit(cancellationToken);

                        _logger.LogInformation($"{Constants.APIErrorMessages.TEAM_UPDATED_SUCESSFULLY}");
                        return new RemoveOrBanPlayerResponse { Message = Constants.APIErrorMessages.TEAM_UPDATED_SUCESSFULLY };
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
        /// Invite player to join team
        /// </summary>
        /// <param name="userId">user id who requesting the api</param>
        /// <param name="email">user email who requesting the api</param>
        /// <param name="invitePlayerToTeam">contains player email/username/phonenumber and team id to join</param>
        /// <returns>problem statement incase of error and message string incase of success</returns>
        public async Task<ErrorOr<GenericMessage>> InvitePlayerToJoinTeamAsync(long userId, string email, InvitePlayerToTeam invitePlayerToTeam)
        {
            TeamMapper teamMapper = new TeamMapper();
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                Domain.Entities.UserInfo userInfo = _iuserInfoRepository.GetUserInfoByUserInfoId(invitePlayerToTeam.PlayerId);
                Domain.Entities.UserInfo captainInfo = _iuserInfoRepository.GetUserInfoByUserInfoId(userId);
                if (userInfo is null)
                {
                    return Domain.Common.Errors.Errors.User.ErrorUserNotFound;
                }
                else
                {
                    NotificationMapper notificationMapper = new NotificationMapper();
                    //TeamMember teamMemberExisting = await _iteamMemberRepository.GetTeamMemberRequestAcceptedButDeletedByTeamIdAndPlayerId(invitePlayerToTeam.TeamId, userInfo.Id);
                    var teamMemberRoleId = await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Member.ToString());

                    //if (teamMemberExisting is not null)
                    //{
                    //    teamMemberExisting.TeamMemberRoleId = teamMemberRoleId.Id;
                    //    teamMemberExisting.IsDeleted = false;
                    //    teamMemberExisting.IsAccepted = false;
                    //    teamMemberExisting.IsActive = true;

                    //    await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(teamMemberExisting);
                    //    await _iunitOfWork.Commit(cancellationToken);
                    //    _logger.LogInformation("TeamMember is updated successfully {@teamMemberExisting}", teamMemberExisting);
                    //}
                    //else
                    //{

                    TeamMember teamMember = teamMapper.MapTeamMemberWithTeam(invitePlayerToTeam.TeamId, email, teamMemberRoleId.Id, userInfo.Id);
                    teamMember.IsAccepted = false;
                    var teamName = await _context.Team.Where(x => x.Id == teamMember.TeamId).Select(x => x.Name).FirstOrDefaultAsync();
                    //var teamName = "Pakistan";

                    teamMember = await _iunitOfWork.Repository<Domain.Entities.TeamMember>().AddAsync(teamMember);

                    Common.GetNotificationMsg notificationMessage = Common.GetNotificationMessages.GetNotificationMessage.GetTeamMembetoJoinTeamNotification(captainInfo.Name, teamName);

                    var notification = teamMapper.MapTeamMemberWithNotification(invitePlayerToTeam.TeamId, email, userInfo.Id);

                    var notify = notificationMapper.CreateNotification(notificationMessage, Domain.Enums.Enum.NotificationPlatform.Mobile, Domain.Enums.Enum.NotificationsTypes.TeamJoinRequest, email);
                    notify = await _iunitOfWork.Repository<Notifications>().AddAsync(notify);
                    await _iunitOfWork.Commit(cancellationToken);

                    var notifyTo = notificationMapper.CreateNotificationTo(notify, invitePlayerToTeam.PlayerId, email, userId);
                    await _iunitOfWork.Repository<Domain.Entities.NotificationTo>().AddAsync(notifyTo);
                    await _iunitOfWork.Commit(cancellationToken);

                    _logger.LogInformation("TeamMember is created successfully {@teamMember}", teamMember);

                    return new GenericMessage { Message = Constants.APIErrorMessages.REQUEST_CREATED_SUCESSFULLY };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// accept/reject/cancel player invite to join team
        /// </summary>
        /// <param name="userId">user id who requesting the api</param>
        /// <param name="email">user email who requesting the api</param>
        /// <param name="inviteActionToJoinTeam">contains team and player id with enum for invite action</param>
        /// <returns>problem details in case of error and message in case of success</returns>
        public async Task<ErrorOr<GenericMessage>> InviteActionPlayerToJoinTeamAsync(long userId, string email, InviteActionToJoinTeam inviteActionToJoinTeam)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                NotificationMapper notificationMapper = new NotificationMapper();
                TeamMember teamMember = await _iteamMemberRepository.GetTeamMemberRequestNotAcceptedByTeamIdAndPlayerId(userId, inviteActionToJoinTeam.TeamId);
                var teamName = await _context.Team.Where(x => x.Id == teamMember.TeamId).Select(x => x.Name).FirstOrDefaultAsync();
                if (teamMember is null)
                {
                    return Domain.Common.Errors.Errors.Team.NoTeamMemberFound;
                }
                else
                {
                    switch (inviteActionToJoinTeam.InviteAction)
                    {
                        case InviteAction.Accept:
                            var teamJersey = await _iteamMemberRepository.getTeamJersey(inviteActionToJoinTeam.TeamId, userId);

                            //var teamMemberJerseys = _context.TeamMember.Where(x => x.Team.Id == inviteActionToJoinTeam.TeamId)
                            //                       .Select(x => x.JerseyNo).OrderBy(jerseyNo => jerseyNo).ToList();

                            var user = _context.UserInfo.Where(x => x.Id == userId).FirstOrDefault();

                            if (user != null && teamJersey.Contains(user.JerseyNo))
                            {
                                var range = Enumerable.Range(1, 1000).Where(i => !teamJersey.Contains(i.ToString()));
                                var randomJersey = range.Skip(11).Take(1);
                                teamMember.JerseyNo = randomJersey.FirstOrDefault().ToString();
                            }

                            teamMember.IsAccepted = true;
                         


                            await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(teamMember);
                            await _iunitOfWork.Commit(cancellationToken);


                            Common.GetNotificationMsg notificationMessage = Common.GetNotificationMessages.GetNotificationMessage.PlayerAcceptedCaptainRequest(teamName);
                            var notify = notificationMapper.CreateNotification(notificationMessage,
                                Domain.Enums.Enum.NotificationPlatform.Mobile, Domain.Enums.Enum.NotificationsTypes.TeamJoinRequest, email);
                            notify = await _iunitOfWork.Repository<Notifications>().AddAsync(notify);
                            await _iunitOfWork.Commit(cancellationToken);
                            break;

                        case InviteAction.Reject:
                            await _iunitOfWork.Repository<Domain.Entities.TeamMember>().DeleteAsync(teamMember);
                            await _iunitOfWork.Commit(cancellationToken);
                            break;
                        case InviteAction.Cancel:
                            await _iunitOfWork.Repository<Domain.Entities.TeamMember>().DeleteAsync(teamMember);
                            await _iunitOfWork.Commit(cancellationToken);
                            break;
                        default:
                            await _iunitOfWork.Repository<Domain.Entities.TeamMember>().DeleteAsync(teamMember);
                            await _iunitOfWork.Commit(cancellationToken);
                            break;
                    }
                    _logger.LogInformation($"{Constants.APIErrorMessages.RECORD_UPDATED}");
                    return new GenericMessage { Message = Constants.APIErrorMessages.RECORD_UPDATED };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<GenericMessage>> UpdateTeamAsync(long userId, string email, UpdateTeam updateTeam)
        {
            TeamMapper teamMapper = new TeamMapper();
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                var team = await _iteamRepository.GetTeamById(updateTeam.Id);
                var teamActivity = await _iteamActivityRepository.GetTeamActivityByTeamId(updateTeam.Id);

                if (team is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.NoTeamsFound}");
                    return Domain.Common.Errors.Errors.Team.NoTeamsFound;
                }
                else if (teamActivity is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.NoTeamActivityFound}");
                    return Domain.Common.Errors.Errors.Team.NoTeamActivityFound;
                }
                else
                {
                    var eventTypeList = await _iunitOfWork.Repository<Domain.Entities.EventType>().GetAllAsync();
                    var eventTypeId = team.TeamType.Name.ToLower() != Domain.Enums.Enum.TeamType.Temporary.ToString().ToLower() ?
                        eventTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventType.Regulation.ToString().ToLower()).Select(x => x.Id).FirstOrDefault() :
                        eventTypeList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventType.Pickup.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                    team = teamMapper.MapTeamForUpdate(team, updateTeam, email);

                    Domain.Entities.Activity activity = await _iactivityRepository.GetActivityByEventTypeAndActivityCategoryId(eventTypeId, updateTeam.ActivityId);
                    updateTeam.ActivityId = activity.Id;

                    teamActivity = teamMapper.MapTeamActivityForUpdate(teamActivity, updateTeam, email);
                    team = await UpdateTeam(team);

                    _logger.LogInformation("Team updated successfully {@team}", team);

                    await _iunitOfWork.Repository<Domain.Entities.TeamActivity>().UpdateAsync(teamActivity);
                    await _iunitOfWork.Commit(cancellationToken);

                    _logger.LogInformation("Team activity updated successfully {@teamActivity}", teamActivity);

                    return new GenericMessage { Message = Constants.APIErrorMessages.TEAM_UPDATED_SUCESSFULLY };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<List<GetAllInvites>>> GetAllInvitesToJoinTeamAsync(long teamId)
        {
            try
            {
                List<GetAllInvites> getAllInviteList = await _iteamRepository.GetAllInvitesToJoinTeamList(teamId);
                return getAllInviteList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// return teams with user access
        /// </summary>
        /// <param name="userId">user id who is requesting</param>
        /// <returns></returns>
        public async Task<ErrorOr<List<TeamGenericObj>>> GetTeamForDropdownAsync(long userId, Domain.Enums.Enum.TeamType teamType, long tournamentId)
        {
            try
            {
                List<TeamGenericObj> teamList = await _iteamRepository.GetTeamForDropdownAsync(userId, teamType, tournamentId);
                return teamList.Any() ? teamList : Domain.Common.Errors.Errors.Team.NoTeamsFound;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<List<PlayerDetails>>> GetAllPlayerAsync(long userId, long teamId, string sSearch)
        {
            try
            {
                List<PlayerDetails> playerDetailList = await _iteamMemberRepository.GetAllPlayerAsyncByTeamId(teamId, sSearch);

                return playerDetailList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        /// <summary>
        /// get events and tournaments by team id
        /// </summary>
        /// <param name="teamId">team which details required</param>
        /// <param name="userId">user id who is requesting the api</param>
        /// <returns>model in case of success and problem statement in case of error</returns>
        public async Task<ErrorOr<GetTeamDetails>> GetTeamDetailsByTeamIdAsync(long teamId, long userId, int lastDays)
        {
            TeamMapper teamMapper = new TeamMapper();
            try
            {
                var team = await _iunitOfWork.Repository<Domain.Entities.Team>().GetByIdAsync(teamId);

                if (team is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.NoTeamsFound}");
                    return Domain.Common.Errors.Errors.Team.NoTeamsFound;
                }
                else
                {
                    GetTeamDetails getTeamEvent = await _iteamRepository.GetTeamsDetailByTeamId(userId, teamId);

                    _logger.LogInformation("Response {@getTeamEvent}", getTeamEvent);
                    return getTeamEvent;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<List<MemberDetail>>> GetPlayersDetailsByTeamIdAsync(long teamId, long userId, string sSearch, bool isCaptain)
        {
            try
            {
                List<MemberDetail> memberDetailList = await _iteamRepository.GetPlayersDetailsByTeamId(userId, teamId, sSearch, isCaptain);
                _logger.LogInformation("memberDetailList {@memberDetailList}", memberDetailList);
                return memberDetailList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<GetTeamPlayersStat>> GetPlayersStatsByTeamIdAsync(long teamId, long userId)
        {
            TeamMapper teamMapper = new TeamMapper();
            try
            {
                var team = await _iteamRepository.GetTeamById(teamId);

                if (team is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.NoTeamsFound}");
                    return Domain.Common.Errors.Errors.Team.NoTeamsFound;
                }
                else
                {
                    List<TeamMember> teamMemberList = await _iteamMemberRepository.GetTeamMembersListByTeamId(teamId);
                    List<long> teamPlayersIdsList = teamMemberList.Select(x => x.PlayerId.Value).ToList();
                    List<TeamMemberMatchesWon> teamMemberMatchesWonList = await _iteamMemberRepository.GetTeamMembersMatchesWonList(userId, teamPlayersIdsList);
                    List<Domain.Entities.Images> memberImagesList = await _iimagesRepository.GetUserProfileImagesByUserIdList(teamPlayersIdsList);
                    List<EventTeamMemberStatResult> eventTeamMemberStatResultList = await _iteamRepository.GetPlayersStatsByTeamId(userId, teamId);

                    int win = await _iteamRepository.GetMatchesWonByTeamId(teamId, 0);
                    int lose = await _iteamRepository.GetMatchesLoseByTeamId(teamId, 0);
                    int tie = await _iteamRepository.GetMatchesTieByTeamId(teamId, 0);
                    DateTime lastPlayedDateTime = await _iteamRepository.GetLastPlayedDateTimeByTeamId(teamId);

                    GetTeamPlayersStat getTeamPlayersStat = teamMapper.MapTeamMembersStatsByTeam(team, teamMemberList, eventTeamMemberStatResultList, win,
                        lose, tie, memberImagesList, teamMemberMatchesWonList, lastPlayedDateTime);

                    _logger.LogInformation("getTeamPlayersStat {@getTeamPlayersStat}", getTeamPlayersStat);
                    return getTeamPlayersStat;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
        public async Task<ErrorOr<List<GetTeamDetailsByEventCollectionRes>>> GetTemporaryTeamsByEventCollection(long userId, string email, long eventCollectionId, long eventId, long roleId)
        {
            TeamMapper teamMapper = new TeamMapper();

            try
            {
                _logger.LogInformation("Getting Team from EventCollection");
                var teams = eventId != 0 ? await _iteamRepository.GetTeamsByEventId(eventId, userId) : await _iteamRepository.GetTeamsByEventCollection(eventCollectionId, userId);

                List<WaitingList> waitingList = await _ideviceReposiroty.GetWaitingListDeletePlayerList(eventCollectionId);
                var players = teams.Count > 0 ? await _iteamRepository.GetPlayersbyTeamId(teams.Select(x => x.teamId).Distinct().ToList()) : null;
                var results = players != null ? teamMapper.GetTeamsByEventCollectionId(teams, players, waitingList, eventId) : null;

                return results != null ? results : Domain.Common.Errors.Errors.User.ErrorNoTeamFound;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<GenericMessage>> BackToLobby(long playerId, long teamId, long eventCollectionId, string email)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            WaitingListMapper waitingListMapper = new WaitingListMapper();

            try
            {
                var isPresent = await _iteamMemberRepository.GetTeamMemberRequestAcceptedByTeamIdAndPlayerId(teamId, playerId);

                if (teamId == 0)
                {
                    var waitingListupdate = await _ideviceReposiroty.GetWaitingListByPlayerId(playerId, eventCollectionId);
                    var checkPlayerIsAlreadyAPartOfAnyTeam = await _iteamMemberRepository.CheckIfPlayerIsAMemberOfAnyTeamInEventCollectionById(playerId, eventCollectionId);

                    if (checkPlayerIsAlreadyAPartOfAnyTeam is not null && checkPlayerIsAlreadyAPartOfAnyTeam.IsActive != false && checkPlayerIsAlreadyAPartOfAnyTeam.IsDeleted != true && checkPlayerIsAlreadyAPartOfAnyTeam.IsJoinedCollection != false)
                    {
                        _logger.LogInformation($"{Constants.APIErrorMessages.PLAYER_ALREADY_A_MEMBER}");
                        return new GenericMessage { Message = Constants.APIErrorMessages.PLAYER_ALREADY_A_MEMBER };
                    }
                    else if (waitingListupdate is not null)
                    {
                        waitingListupdate.IsActive = true;
                        waitingListupdate.IsDeleted = false;

                        await _iunitOfWork.Repository<Domain.Entities.WaitingList>().UpdateAsync(waitingListupdate);
                        await _iunitOfWork.Commit(cancellationToken);

                        _logger.LogInformation($"{Constants.APIErrorMessages.RECORD_UPDATED}");
                        return new GenericMessage { Message = Constants.APIErrorMessages.RECORD_UPDATED };
                    }
                    else
                    {
                        var lastWaitingListPlayer = await _ideviceReposiroty.GetLastPlayerNumberInTheList(eventCollectionId);
                        Domain.Entities.WaitingList waitingList = waitingListMapper.CreateWaitingListMap(email, playerId, eventCollectionId, lastWaitingListPlayer);

                        await _iunitOfWork.Repository<Domain.Entities.WaitingList>().AddAsync(waitingList);
                        await _iunitOfWork.Commit(cancellationToken);

                        _logger.LogInformation($"{Constants.APIErrorMessages.RECORD_UPDATED}");
                        return new GenericMessage { Message = Constants.APIErrorMessages.RECORD_UPDATED };
                    }
                }
                else if (isPresent != null)
                {
                    using (var dbContextTransaction = _context.Database.BeginTransaction())
                    {
                        var checkTeamType = isPresent.Team.TeamType.Name;
                        var waitingListupdate = await _ideviceReposiroty.GetWaitingListByPlayerIdDeletedOne(playerId, eventCollectionId);

                        waitingListupdate.IsDeleted = false;
                        waitingListupdate.IsActive = true;

                        await _iunitOfWork.Repository<Domain.Entities.WaitingList>().UpdateAsync(waitingListupdate);
                        await _iunitOfWork.Commit(cancellationToken);

                        var checkTheEventStatus = await _ieventTeamRepository.GetEventTeamByEventCollectionIdAndTeamIdAsync(eventCollectionId, teamId);

                        if (checkTheEventStatus is not null && checkTheEventStatus.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Active.ToString().ToLower())
                        {
                            //Domain.Entities.TeamMemberRole memberRole = await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Member.ToString());
                            isPresent.IsSubstituteOrDisqualify = true;
                            //isPresent.TeamMemberRoleId = memberRole.Id;
                        }
                        else if (checkTeamType.ToLower() != Domain.Enums.Enum.TeamType.Official.ToString().ToLower())
                        {
                            isPresent.IsDeleted = true;
                            isPresent.IsActive = false;
                        }
                        else
                        {
                            isPresent.IsJoinedCollection = false;
                        }

                        await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(isPresent);
                        await _iunitOfWork.Commit(cancellationToken);

                        long teamIdForMemberRoleUpdate = teamId;

                        //updating next member in the team as captain
                        TeamMember teamMemberToMakeCaptain = await _iteamMemberRepository.GetTeamMemberByTeamIdNotSubstituteFirstOrDefault(teamIdForMemberRoleUpdate);

                        if (teamMemberToMakeCaptain is not null && teamMemberToMakeCaptain.Team.TeamType.Name.ToLower() != Domain.Enums.Enum.TeamType.Official.ToString().ToLower())
                        {
                            Domain.Entities.TeamMemberRole captainRole = await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Captain.ToString());
                            teamMemberToMakeCaptain.TeamMemberRoleId = captainRole.Id;

                            await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(teamMemberToMakeCaptain);
                            await _iunitOfWork.Commit(cancellationToken);

                            _logger.LogInformation($"new captain assign successfully");
                        }

                        if (checkTeamType.ToLower() != Domain.Enums.Enum.TeamType.Official.ToString().ToLower())
                        {
                            long eventCollectionIdd = await _ieventCollectionTeamRepository.GetEventCollectionIdByTeamId(teamId);
                            await AssignSequenceToTheTeamRemove(eventCollectionIdd, teamId);
                        }

                        //check if this teamMember team have zero members
                        int countTeamMember = _iteamMemberRepository.GetTeamMemberCountByTeamId(teamId);
                        if (countTeamMember == 0 && checkTeamType.ToLower() != Domain.Enums.Enum.TeamType.Official.ToString().ToLower())
                        {
                            var deleteTeam = await _iunitOfWork.Repository<Domain.Entities.Team>().GetByIdAsync(teamId);
                            if (deleteTeam is not null)
                            {
                                await _iteamRepository.DeleteTeamById(deleteTeam);
                                _logger.LogInformation($"delete team successfully because team member count is {countTeamMember}");
                            }
                        }

                        dbContextTransaction.Commit();
                        _logger.LogInformation($"{Constants.APIErrorMessages.RECORD_UPDATED}");
                        return new GenericMessage { Message = Constants.APIErrorMessages.RECORD_UPDATED };
                    }
                }
                else
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.NoTeamMemberFound}");
                    return Domain.Common.Errors.Errors.Team.NoTeamMemberFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<GenericMessage>> AssignCaptainAsync(long userId, string email, AssignCaptainToTeam assignCaptainToTeam)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                TeamMember teamMember = await _iteamMemberRepository.GetCaptainOfTheTeam(assignCaptainToTeam.TeamId);
                TeamMember teamMemberToBeCaptain = await _iteamMemberRepository.GetTeamMemberRequestAcceptedByTeamIdAndPlayerId(assignCaptainToTeam.TeamId, assignCaptainToTeam.PlayerId);

                if (teamMember is null)
                {
                    return Domain.Common.Errors.Errors.Team.NoTeamMemberFound;
                }
                else if (teamMemberToBeCaptain is null)
                {
                    return Domain.Common.Errors.Errors.Team.NoTeamMemberFound;
                }
                else
                {
                    teamMember.IsDeleted = true;
                    teamMember.IsActive = false;

                    await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(teamMember);
                    await _iunitOfWork.Commit(cancellationToken);

                    _logger.LogInformation($"Team member {teamMember.PlayerId} is removed from team {assignCaptainToTeam.TeamId}");

                    var teamMemberRoleId = await _iteamMemberRoleRepository.GetTeamMemberRoleByName(Domain.Enums.Enum.TeamMemberRole.Captain.ToString());


                    teamMemberToBeCaptain.TeamMemberRoleId = teamMemberRoleId.Id;

                    await _iunitOfWork.Repository<Domain.Entities.TeamMember>().UpdateAsync(teamMemberToBeCaptain);
                    await _iunitOfWork.Commit(cancellationToken);

                    _logger.LogInformation($"New Captain is assign successfully {teamMemberToBeCaptain.PlayerId}");

                    return new GenericMessage { Message = Constants.APIErrorMessages.TEAM_UPDATED_SUCESSFULLY };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<GenericMessage>> DeleteTeamAsync(long userId, string email, long teamId = 0)
        {
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                TeamMember isCaption = await _iteamMemberRepository.checkCaption(userId);
                Domain.Entities.Team team = await _iunitOfWork.Repository<Domain.Entities.Team>().GetByIdAsync(teamId);

                if (isCaption is not null && isCaption.TeamMemberRoleId != ((int)Domain.Enums.Enum.TeamMemberRole.Captain))
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.PermissionDenied}");
                    return Domain.Common.Errors.Errors.Team.PermissionDenied;
                }
                else if (team is null)
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.NoTeamsFound}");
                    return Domain.Common.Errors.Errors.Team.NoTeamsFound;
                }
                else
                {
                    team = await _iteamRepository.DeleteTeamById(team);

                    _logger.LogInformation($"{Constants.APIErrorMessages.TEAM_UPDATED_SUCESSFULLY}");
                    return new GenericMessage { Message = Constants.APIErrorMessages.TEAM_UPDATED_SUCESSFULLY };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<GetAllMembersListInvite>> GetInviteMembersDetailsAsync(long userId, long teamId, string sSearch)
        {
            TeamMapper teamMapper = new TeamMapper();
            try
            {
                int recentMatchPlayedCount = 10;
                TeamMember isCaption = await _iteamMemberRepository.checkCaption(userId);

                if (isCaption is not null && isCaption.TeamMemberRole.Name.ToLower() != Domain.Enums.Enum.TeamMemberRole.Captain.ToString().ToLower())
                {
                    _logger.LogWarning($"{Domain.Common.Errors.Errors.Team.PermissionDenied}");
                    return Domain.Common.Errors.Errors.Team.PermissionDenied;
                }
                else
                {
                    List<long> playerProfileImageList = new List<long>();
                    List<TeamMember> totalTeamMemberList = await _iteamMemberRepository.GetTeamMemberRequestByTeamId(teamId);
                    List<long> totalTeamMemberIdList = totalTeamMemberList.Select(x => x.PlayerId.Value).ToList();

                    List<TeamMember> invitedNotAcceptedList = await _iteamMemberRepository.GetTeamMemberListRequestNotAcceptedByTeamId(teamId);
                    playerProfileImageList = invitedNotAcceptedList.Select(x => x.PlayerId.Value).ToList();

                    List<Follow> followList = await _ifollowRepository.GetAllFollowerOrFolloweeList(userId, playerProfileImageList);
                    playerProfileImageList.AddRange(followList.Select(x => x.FollowerId.Value).ToList());
                    playerProfileImageList.AddRange(followList.Select(x => x.FolloweeId.Value).ToList());

                    List<TeamMember> recentlyPlayedWithMatchPlayerList = await _iteamMemberRepository.GetTeamMembersListWhichPlayedWithThisTeamIdLastMatchCount(userId,
                        recentMatchPlayedCount, totalTeamMemberIdList);
                    playerProfileImageList.AddRange(recentlyPlayedWithMatchPlayerList.Select(x => x.PlayerId.Value).ToList());

                    List<Domain.Entities.Images> profileImageList = await _iimagesRepository.GetUserProfileImagesByUserIdList(playerProfileImageList);

                    GetAllMembersListInvite getAllMembersList = teamMapper.MapInvitedMemberListAndRecentlyPlayer(invitedNotAcceptedList, followList,
                                                                recentlyPlayedWithMatchPlayerList, userId, profileImageList, sSearch);

                    return getAllMembersList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }

        public async Task<ErrorOr<GetAllAvgResp>> GetAllAvg(long userId, long tornamentId, long VenueId, long eventType)
        {
            GetAllAvgResp getAllAvgResp = new GetAllAvgResp();
            try
            {
                TeamMapper teamMapper = new TeamMapper();
                if (VenueId != 0)
                {
                    var getVenue = await _iteamMemberRepository.GetVenuesForAvg(userId, VenueId);
                    List<long> eventListId = getVenue.Select(v => v.eventId).ToList();

                    var getAvg = await _iteamMemberRepository.GetAvgOnEvent(userId, eventListId);
                    var avgeragesVenue = teamMapper.GetAvgMapper(getAvg);
                    return avgeragesVenue;
                }
                else if (tornamentId != 0)
                {
                    var team = _iteamMemberRepository.GetTeamMemberVsTournament(tornamentId, userId);
                    var teamId = team.Result;

                    var eventId = _iteamMemberRepository.GetEventVsTournament(teamId, userId);
                    var getAvg = await _iteamMemberRepository.GetAvgOnEvent(userId, eventId);

                    var avgeragesBytournaments = teamMapper.GetAvgMapper(getAvg);
                    return avgeragesBytournaments;
                }
                else if (eventType != 0)
                {
                    var team = _iteamMemberRepository.GetEventsaByEventType(eventType, userId);
                    var eventId = team.Result;
                    var getAvg = await _iteamMemberRepository.GetAvgOnEvent(userId, eventId);

                    var avgeragesBytournaments = teamMapper.GetAvgMapper(getAvg);
                    return avgeragesBytournaments;
                }

                else
                {
                    var tournamentEvents = await _iteamMemberRepository.GetAvgTournamentEvents(userId, tornamentId, VenueId);
                    var avgerages = teamMapper.GetAvgMapper(tournamentEvents);
                    return avgerages;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.NoDataFound;
            }
        }
    }
}