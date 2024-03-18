using Azure;
using Ensuranx.Application.Requests.Event;
using Ensuranx.Application.Response.Branch;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.Team;
using Ensuranx.Application.Response.Venue;
using Ensuranx.Common.Constants;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Mapster;
//using Microsoft.AspNetCore.Mvc.Internal;
using System;
using System.Linq;
using static Google.Apis.Requests.BatchRequest;

namespace Ensuranx.Infrastructure.Mappers
{
    public class EventMapper
    {
        public string MvpFormula { get; set; } = "1.5*Points+1.25*Assists+1.15*Rebounds+1.75*Steals+1.75*Blocks+0.5*TotalTeamPt";
        public string WinScoreFormula { get; } = "TotalPoints/2";
        public string LoseScoreFormula { get; } = "0";
        public string MvpPercentage { get; set; } = "# of times as mvp / # of games played";
        public string WinPercentage { get; set; } = "# of games won / # of games played";

        public Event MapEventForCreate(string email, long statkeeperId, DateTime eventDateTime)
        {
            Event @event = new Event();

            @event.StatKeeperId = statkeeperId != 0 ? statkeeperId : null;
            @event.Name = string.Empty;
            @event.CreatedDateTime = DateTime.UtcNow;
            @event.LastModifiedDateTime = DateTime.UtcNow;
            @event.CreatedBy = email;
            @event.LastModifiedBy = email;
            @event.EventDateTime = eventDateTime;

            return @event;
        }

        public List<EventTeam> MapEventTeamForCreate(string email, long eventId, List<EventCollectionTeam> eventCollectionTeamList, Domain.Entities.EventStatus eventStatus)
        {
            List<EventTeam> eventTeamList = new List<EventTeam>();

            foreach (EventCollectionTeam eventCollectionTeam in eventCollectionTeamList)
            {
                EventTeam eventTeam = new EventTeam();

                eventTeam.EventStatusId = eventStatus.Id;
                eventTeam.EventCollectionTeamId = eventCollectionTeam.Id;
                eventTeam.EventId = eventId;
                eventTeam.CreatedDateTime = DateTime.UtcNow;
                eventTeam.LastModifiedDateTime = DateTime.UtcNow;
                eventTeam.CreatedBy = email;
                eventTeam.LastModifiedBy = email;

                eventTeamList.Add(eventTeam);
            }

            return eventTeamList;
        }

        public EventScoreboad MapEventScoreboard(List<EventTeam> eventTeamList, List<Stat> statList, List<TeamMember> teamMemberList, List<EventLog> eventLogList, List<WaitingList> waitingList)
        {
            EventScoreboad eventScoreboad = new EventScoreboad();

            eventScoreboad.GetTeamPlayerList = new List<GetTeamPlayers>();

            foreach (EventTeam eventTeam in eventTeamList)
            {
                GetTeamPlayers getTeamPlayers = new GetTeamPlayers();

                getTeamPlayers.Id = eventTeam.EventCollectionTeam.Team.Id;
                getTeamPlayers.TeamName = eventTeam.EventCollectionTeam.Team.Name;
                getTeamPlayers.Colour = eventTeam.EventCollectionTeam.Team.Colour;

                var getCurrentTeamPoints = eventLogList.Where(x => x.TeamMember.TeamId == getTeamPlayers.Id).ToList();

                getTeamPlayers.Point = CalculatePoints(getCurrentTeamPoints);

                List<TeamMember> teamMemList = teamMemberList.Where(x => x.TeamId == getTeamPlayers.Id).ToList();
                getTeamPlayers.PlayerDetailList = new List<PlayerDetails>();

                foreach (TeamMember teamMember in teamMemList)
                {
                    PlayerDetails playerDetail = new PlayerDetails();

                    var currentTeamMemberStat = eventLogList.Where(x => x.TeamMemberId == teamMember.Id).ToList();
                    int playerSequenceNumber = waitingList.Where(x => x.UserInfoId == teamMember.PlayerId.Value).Select(x => x.PlayerNumber).FirstOrDefault();

                    playerDetail.Id = teamMember.Id;
                    playerDetail.PlayerId = teamMember.PlayerId.Value;
                    playerDetail.Name = teamMember.UserInfo.Name;
                    playerDetail.Role = teamMember.TeamMemberRole.Name;
                    playerDetail.SequenceNumber = playerSequenceNumber < 10 ? "0" + playerSequenceNumber.ToString() : playerSequenceNumber.ToString();
                    
                    playerDetail.BasketballEnsuranxList = new List<BasketballEnsuranx>();

                    foreach (Stat stat in statList)
                    {
                        BasketballEnsuranx basketballEnsuranx = new BasketballEnsuranx();

                        basketballEnsuranx.Id = stat.Id;
                        basketballEnsuranx.Name = stat.Name;
                        basketballEnsuranx.Type = stat.StatType.Name;
                        basketballEnsuranx.NumberOfHits = currentTeamMemberStat.Where(x => x.StatId == stat.Id && x.IsFix != true && x.IsSucessfull != false && x.IsPause == 0 && x.IsShotClock != true).Count();
                        basketballEnsuranx.NumberOfMiss = currentTeamMemberStat.Where(x => x.StatId == stat.Id && x.IsSucessfull != true && x.IsPause == 0 && x.IsShotClock != true).Count();

                        playerDetail.BasketballEnsuranxList.Add(basketballEnsuranx);
                    }

                    getTeamPlayers.PlayerDetailList.Add(playerDetail);
                }
                eventScoreboad.GetTeamPlayerList.Add(getTeamPlayers);
            }

            return eventScoreboad;
        }

        public EventLog MapEventLog(string email, UpdateEventScoreboard updateEventScoreboard)
        {
            EventLog eventLog = new EventLog();

            eventLog = updateEventScoreboard.Adapt<EventLog>();

            eventLog.CreatedBy = email;
            eventLog.LastModifiedBy = email;
            eventLog.CreatedDateTime = DateTime.UtcNow;
            eventLog.LastModifiedDateTime = DateTime.UtcNow;

            return eventLog;
        }

        public EventResultResponse MapEventResult(List<EventTeamMemberStatResult> eventTeamMemberStatResultList,
            List<TeamMember> teamMemberList, List<EventTeam> eventTeamList, Images imageStatkeeper, Images imageActivity, List<Images> memberImagesList,
            List<EventPlayedByPlayerCount> eventPlayedByPlayerCountList, List<EventTeamMemberStatResult> previousRecordOfTeamMembers, List<WaitingList> waitingList, int totalEventDuration,
            EventDurationModel eventDurationModel)
        {
            EventResultResponse response = new EventResultResponse();

            response.BranchName = eventTeamList.First().EventCollectionTeam.EventCollection.Venue.Branch.Name;
            response.EventDateTime = eventTeamList.First().Event.EventDateTime;
            response.StatkeeperName = eventTeamList.First().EventCollectionTeam.EventCollection.UserInfo.Name;
            response.StatkeeperImage = imageStatkeeper != null ? imageStatkeeper.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
            response.ActivityIcon = imageActivity != null ? imageActivity.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
            response.EventDuration = eventDurationModel.eventDurationMinute;
            response.EventDurationInMinute = eventDurationModel.eventDurationMinute;
            response.EventDurationInSecond = eventDurationModel.eventDurationSecond;
            response.TotalEventDurationInMinutes = totalEventDuration;

            List<BasketballResultResponse> basketballResultResponseList = new List<BasketballResultResponse>();

            for (int i = 0; i < eventTeamList.Count; i++)
            {
                BasketballResultResponse basketballResultResponse = new BasketballResultResponse();

                basketballResultResponse.TeamId = eventTeamList[i].EventCollectionTeam.TeamId;
                basketballResultResponse.TeamName = eventTeamList[i].EventCollectionTeam.Team.Name;
                basketballResultResponse.EventStatus = eventTeamList[i].EventStatus.Name;
                basketballResultResponse.Points = eventTeamList[i].Points;
                basketballResultResponse.Colour = eventTeamList[i].EventCollectionTeam.Team.Colour;

                List<TeamMember> currentTeamMembers = teamMemberList.Where(x => x.TeamId == eventTeamList[i].EventCollectionTeam.TeamId).ToList();
                basketballResultResponse.basketballPlayerMatchSummaryList = new List<BasketballPlayerMatchSummary>();

                foreach (TeamMember teamMember in currentTeamMembers)
                {
                    BasketballPlayerMatchSummary basketballPlayerMatchSummary = new BasketballPlayerMatchSummary();

                    EventTeamMemberStatResult currentMemberEventResult = eventTeamMemberStatResultList.Where(x => x.TeamMemberId == teamMember.Id && x.EventId == eventTeamList[i].EventId).FirstOrDefault();
                    long gP = eventPlayedByPlayerCountList.Where(x => x.PlayerId == teamMember.PlayerId).Select(x => x.GamesPlayedCount).FirstOrDefault();
                    
                    int getPlayerSequence = waitingList.Where(x => x.UserInfoId == teamMember.PlayerId.Value).Select(x => x.PlayerNumber).FirstOrDefault();

                    List<EventTeamMemberStatResult> prevoiusRecordOfMember = previousRecordOfTeamMembers.Where(x => x.TeamMember.PlayerId.Value == teamMember.PlayerId.Value).ToList();

                    int mvpCount = prevoiusRecordOfMember.Where(x => x.IsMvp != false).Count();

                    Images getTeamMemberImage = memberImagesList.Where(x => x.TableId == teamMember.PlayerId).FirstOrDefault();
                    basketballPlayerMatchSummary.Id = teamMember.Id;
                    basketballPlayerMatchSummary.PlayerName = teamMember.UserInfo.Name;
                    basketballPlayerMatchSummary.PlayerRole = teamMember.TeamMemberRole.Name;
                    basketballPlayerMatchSummary.IsSubstituteOrDisqualify = teamMember.IsSubstituteOrDisqualify;
                    basketballPlayerMatchSummary.ProfileImage = getTeamMemberImage != null ? getTeamMemberImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
                    //mapping calculated points
                    if (currentMemberEventResult is not null)
                    {
                        basketballPlayerMatchSummary.Pts = currentMemberEventResult.TotalPoint;
                        basketballPlayerMatchSummary.Ast = currentMemberEventResult.Assist;
                        basketballPlayerMatchSummary.Rebs = currentMemberEventResult.Rebound;
                        basketballPlayerMatchSummary.Stls = currentMemberEventResult.Steal;
                        basketballPlayerMatchSummary.Blks = currentMemberEventResult.Block;
                        basketballPlayerMatchSummary.TotalScore = currentMemberEventResult.TotalScore;
                        basketballPlayerMatchSummary.WinScore = currentMemberEventResult.WinScore;
                        basketballPlayerMatchSummary.MvpPercentage = string.Concat(currentMemberEventResult.MvpPerc.ToString(), "%");
                        basketballPlayerMatchSummary.WinPercentage = string.Concat(currentMemberEventResult.WinPerc.ToString(), "%");
                        basketballPlayerMatchSummary.IsMvp = currentMemberEventResult.IsMvp;
                        basketballPlayerMatchSummary.Gp = gP;
                        basketballPlayerMatchSummary.MvpCount = mvpCount;
                        basketballPlayerMatchSummary.SequenceNumber = getPlayerSequence < 10 ? "0" + getPlayerSequence.ToString() : getPlayerSequence.ToString();

                        basketballResultResponse.basketballPlayerMatchSummaryList.Add(basketballPlayerMatchSummary);
                        basketballResultResponse.basketballPlayerMatchSummaryList = basketballResultResponse.basketballPlayerMatchSummaryList.OrderBy(x => x.IsSubstituteOrDisqualify).ToList();
                    }
                }

                basketballResultResponseList.Add(basketballResultResponse);
            }
            response.BasketballResultResponseList = new List<BasketballResultResponse>();
            response.BasketballResultResponseList = basketballResultResponseList;

            return response;
        }
         public EventResultResponse MapActiveEventResult(List<EventLog> eventLogList,
            List<TeamMember> teamMemberList, List<EventTeam> eventTeamList, Images imageStatkeeper, Images imageActivity, List<Images> memberImagesList,
            List<EventPlayedByPlayerCount> eventPlayedByPlayerCountList, List<EventTeamMemberStatResult> previousRecordOfTeamMembers, List<WaitingList> waitingList, List<TeamMemberMatchesWin> numberOfGamesWonList,
            EventDurationModel eventDurationModel,int totalEventDuration)
        {
            EventResultResponse response = new EventResultResponse();

            response.BranchName = eventTeamList.First().EventCollectionTeam.EventCollection.Venue.Branch.Name;
            response.EventDateTime = eventTeamList.First().Event.EventDateTime;
            response.StatkeeperName = eventTeamList.First().EventCollectionTeam.EventCollection.UserInfo.Name;
            response.StatkeeperImage = imageStatkeeper != null ? imageStatkeeper.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
            response.ActivityIcon = imageActivity != null ? imageActivity.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
            response.EventDuration = eventDurationModel.eventDurationMinute;
            response.EventDurationInMinute = eventDurationModel.eventDurationMinute;
            response.EventDurationInSecond = eventDurationModel.eventDurationSecond;
            response.TotalEventDurationInMinutes = totalEventDuration;
            response.NumberOfShotClock = eventLogList.Where(x => x.IsShotClock != false).Count();

            bool isEventPause = eventLogList.LastOrDefault() is not null && eventLogList.LastOrDefault().IsPause == 1 ? true : false;

            response.IsEventPause = isEventPause;

            var lastShotClockTime = eventLogList.Where(x => x.IsShotClock != false).LastOrDefault();
            if (lastShotClockTime is not null)
            {
                var totalSecondsPassedSinceLastShotClock = Math.Round(DateTime.UtcNow.Subtract(lastShotClockTime.CreatedDateTime).TotalSeconds);

                response.ShotClockTimer = totalSecondsPassedSinceLastShotClock <= Constants.APIErrorMessages.SHOT_CLOCK_TIMER ? Constants.APIErrorMessages.SHOT_CLOCK_TIMER - totalSecondsPassedSinceLastShotClock : 0;
            }
            

            List<BasketballResultResponse> basketballResultResponseList = new List<BasketballResultResponse>();

            for (int i = 0; i < eventTeamList.Count; i++)
            {
                BasketballResultResponse basketballResultResponse = new BasketballResultResponse();

                basketballResultResponse.TeamId = eventTeamList[i].EventCollectionTeam.TeamId;
                basketballResultResponse.TeamName = eventTeamList[i].EventCollectionTeam.Team.Name;
                basketballResultResponse.EventStatus = eventTeamList[i].EventStatus.Name;
                //basketballResultResponse.Points = eventTeamList[i].Points;
                basketballResultResponse.Colour = eventTeamList[i].EventCollectionTeam.Team.Colour;

                List<TeamMember> currentTeamMembers = teamMemberList.Where(x => x.TeamId == eventTeamList[i].EventCollectionTeam.TeamId).ToList();
                basketballResultResponse.basketballPlayerMatchSummaryList = new List<BasketballPlayerMatchSummary>();

                foreach (TeamMember teamMember in currentTeamMembers)
                {
                    BasketballPlayerMatchSummary basketballPlayerMatchSummary = new BasketballPlayerMatchSummary();

                    List<EventLog> currentMemberEventResult = eventLogList.Where(x => x.TeamMemberId == teamMember.Id && x.EventId == eventTeamList[i].EventId).ToList();
                    long gP = eventPlayedByPlayerCountList.Where(x => x.PlayerId == teamMember.PlayerId).Select(x => x.GamesPlayedCount).FirstOrDefault();
                    
                    int getPlayerSequence = waitingList.Where(x => x.UserInfoId == teamMember.PlayerId.Value).Select(x => x.PlayerNumber).FirstOrDefault();

                    List<EventTeamMemberStatResult> prevoiusRecordOfMember = previousRecordOfTeamMembers.Where(x => x.TeamMember.PlayerId.Value == teamMember.PlayerId.Value).ToList();

                    int numberOfGamesWon = numberOfGamesWonList
                                       .Where(x => x.TeamMember.PlayerId == teamMember.PlayerId
                                        && x.EventTeam.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower())
                                       .Count();

                    int mvpCount = prevoiusRecordOfMember.Where(x => x.IsMvp != false).Count();

                    Images getTeamMemberImage = memberImagesList.Where(x => x.TableId == teamMember.PlayerId).FirstOrDefault();

                    basketballPlayerMatchSummary.Id = teamMember.Id;
                    basketballPlayerMatchSummary.PlayerName = teamMember.UserInfo.Name;
                    basketballPlayerMatchSummary.PlayerRole = teamMember.TeamMemberRole.Name;
                    basketballPlayerMatchSummary.IsSubstituteOrDisqualify = teamMember.IsSubstituteOrDisqualify;
                    basketballPlayerMatchSummary.ProfileImage = getTeamMemberImage != null ? getTeamMemberImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;

                    //mapping calculated points
                    if (currentMemberEventResult.Any())
                    {
                        basketballPlayerMatchSummary.Pts = CalculatePoints(currentMemberEventResult);
                        basketballPlayerMatchSummary.Ast = CalculateAssists(currentMemberEventResult);
                        basketballPlayerMatchSummary.Rebs = CalculateRebounds(currentMemberEventResult);
                        basketballPlayerMatchSummary.Stls = CalculateSteals(currentMemberEventResult);
                        basketballPlayerMatchSummary.Blks = CalculateBlocks(currentMemberEventResult);
                        basketballPlayerMatchSummary.TotalScore = CalculateMvpForLive(currentMemberEventResult, basketballPlayerMatchSummary, eventTeamList[i]);
                        basketballPlayerMatchSummary.WinScore = CalculateWinScoreFormula(eventTeamList[i].Points, eventTeamList[i]);
                        
                        basketballPlayerMatchSummary.IsMvp = false;
                    }
                    basketballResultResponse.Points += basketballPlayerMatchSummary.Pts;
                    basketballPlayerMatchSummary.MvpPercentage = Math.Round(CalculateMvpPercentage(mvpCount, Convert.ToInt32(gP)),2).ToString();
                    basketballPlayerMatchSummary.WinPercentage = Math.Round(CalculateWinPercentage(numberOfGamesWon, Convert.ToInt32(gP)),2).ToString();

                    basketballPlayerMatchSummary.Gp = gP;
                    basketballPlayerMatchSummary.MvpCount = mvpCount;
                    basketballPlayerMatchSummary.SequenceNumber = getPlayerSequence < 10 ? "0" + getPlayerSequence.ToString() : getPlayerSequence.ToString();

                    basketballResultResponse.basketballPlayerMatchSummaryList.Add(basketballPlayerMatchSummary);
                    basketballResultResponse.basketballPlayerMatchSummaryList = basketballResultResponse.basketballPlayerMatchSummaryList.OrderBy(x => x.IsSubstituteOrDisqualify).ToList();
                }

                basketballResultResponseList.Add(basketballResultResponse);
            }
            response.BasketballResultResponseList = new List<BasketballResultResponse>();
            response.BasketballResultResponseList = basketballResultResponseList;

            return response;
        }

        public List<EventTeamMemberStatResult> CalculatePercentageForEvent(List<EventTeamMemberStatResult> eventTeamMemberStatResults, 
            List<EventTeamMemberStatResult> previousRecordOfTeamMembers, List<TeamMemberMatchesWin> numberOfGamesWonList)
        {
            foreach (EventTeamMemberStatResult eventTeamMemberStatResult in eventTeamMemberStatResults)
            {
                List<EventTeamMemberStatResult> prevoiusRecordOfMember = previousRecordOfTeamMembers.Where(x => x.TeamMember.PlayerId == eventTeamMemberStatResult.TeamMember.PlayerId).ToList();
                int numberOfGamesPlayed = prevoiusRecordOfMember.Select(x => x.EventId).Distinct().Count();

                int numberOfGamesWon = numberOfGamesWonList
                                       .Where(x => x.TeamMember.PlayerId == eventTeamMemberStatResult.TeamMember.PlayerId
                                        && x.EventTeam.EventStatus.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower())
                                       .Count();

                int numberOfTimesMvp = prevoiusRecordOfMember.Where(x => x.IsMvp != false).Select(x => x.EventId).Distinct().Count();

                eventTeamMemberStatResult.MvpPerc = CalculateMvpPercentage(numberOfTimesMvp, numberOfGamesPlayed);
                eventTeamMemberStatResult.WinPerc = CalculateWinPercentage(numberOfGamesWon, numberOfGamesPlayed);
            }

            return eventTeamMemberStatResults;
        }

        public List<EventTeamMemberStatResult> MapEventTeamMemberStatResult(List<EventLog> eventLogList, List<TeamMember> teamMemberList,
            List<EventTeam> eventTeamList, string email)
        {
            List<EventTeamMemberStatResult> eventTeamMemberStatResultListList = new List<EventTeamMemberStatResult>();
            for (int i = 0; i < eventTeamList.Count; i++)
            {
                List<EventTeamMemberStatResult> eventTeamMemberStatResultList = new List<EventTeamMemberStatResult>();
                List<TeamMember> currentTeamMembers = teamMemberList.Where(x => x.TeamId == eventTeamList[i].EventCollectionTeam.TeamId).ToList();

                foreach (TeamMember teamMember in currentTeamMembers)
                {
                    EventTeamMemberStatResult eventTeamMemberStatResult = new EventTeamMemberStatResult();

                    eventTeamMemberStatResult.EventId = eventTeamList[i].EventId;

                    List<EventLog> currentMemberEventLogs = eventLogList.Where(x => x.TeamMemberId == teamMember.Id).ToList();

                    eventTeamMemberStatResult.TeamMemberId = teamMember.Id;
                    eventTeamMemberStatResult.TeamMember = teamMember;
                    //mapping calculated points
                    eventTeamMemberStatResult.TotalPoint = CalculatePoints(currentMemberEventLogs);
                    eventTeamMemberStatResult.Assist = CalculateAssists(currentMemberEventLogs);
                    eventTeamMemberStatResult.Rebound = CalculateRebounds(currentMemberEventLogs);
                    eventTeamMemberStatResult.Steal = CalculateSteals(currentMemberEventLogs);
                    eventTeamMemberStatResult.Block = CalculateBlocks(currentMemberEventLogs);
                    eventTeamMemberStatResult.TotalScore = CalculateMvp(currentMemberEventLogs, eventTeamMemberStatResult, eventTeamList[i]);
                    eventTeamMemberStatResult.WinScore = CalculateWinScoreFormula(eventTeamList[i].Points, eventTeamList[i]);
                    //eventTeamMemberStatResult.MvpPerc = CalculateMvpPercentage(numberOfTimesMvp, numberOfGamesPlayed);
                    //eventTeamMemberStatResult.WinPerc = CalculateWinPercentage(numberOfGamesWon, numberOfGamesPlayed);
                    eventTeamMemberStatResult.CreatedBy = email;
                    eventTeamMemberStatResult.LastModifiedBy = email;
                    eventTeamMemberStatResult.CreatedDateTime = DateTime.UtcNow;
                    eventTeamMemberStatResult.LastModifiedDateTime = DateTime.UtcNow;

                    eventTeamMemberStatResultList.Add(eventTeamMemberStatResult);
                }
                eventTeamMemberStatResultListList.AddRange(eventTeamMemberStatResultList);
            }

            return eventTeamMemberStatResultListList;
        }


        /// <summary>
        /// calculate points for the particular member
        /// </summary>
        /// <param name="memberEventLogs">Collection of logs for the member</param>
        /// <returns>points</returns>
        private long CalculatePoints(List<EventLog> memberEventLogs)
        {
            long points = 0;

            if (memberEventLogs.Any())
            {
                //filter records to calculate points only 
                memberEventLogs = memberEventLogs.Where(x => x.IsFix != true && x.IsSucessfull != false && x.IsPause == 0 && x.IsShotClock != true &&
                                                             x.Stat.StatType.Name.ToLower() == Domain.Enums.Enum.StatType.HitMiss.ToString().ToLower())
                                                             .ToList();
                //calculating sum of the filtered records
                points = memberEventLogs.Select(x => x.Stat.Value).Sum();
            }

            return points;
        }
        // OverLoad For Points in Match Summary
        //private List<long> CalculatePoints(List<long> eventId)
        //{
        //    List<long> points = new List<long>();

        //    if (eventId.Any())
        //    {
        //        //filter records to calculate points only 
        //        foreach(long i in eventId)
        //        {
        //            var score = 
        //        }
        //    }

        //    return points;
        //}
        /// <summary>
        /// calculate total assist from member
        /// </summary>
        /// <param name="memberEventLogs">Collection of logs for the member</param>
        /// <returns>assists</returns>
        private long CalculateAssists(List<EventLog> memberEventLogs)
        {
            long ast = 0;

            if (memberEventLogs.Any())
            {
                //filter records to calculate points only 
                memberEventLogs = memberEventLogs.Where(x => x.IsFix != true && x.IsPause == 0 && x.IsShotClock != true &&
                                                         x.Stat.StatType.Name.ToLower() == Domain.Enums.Enum.StatType.Counter.ToString().ToLower() &&
                                                         x.Stat.Name.ToLower() == Domain.Enums.Enum.BasketballStat.AST.ToString().ToLower())
                                                         .ToList();
                //calculating sum of the filtered records
                ast = memberEventLogs.Count();
            }
            return ast;
        }

        /// <summary>
        /// calculate total rebounds from member
        /// </summary>
        /// <param name="memberEventLogs">Collection of logs for the member</param>
        /// <returns>rebounds</returns>
        private long CalculateRebounds(List<EventLog> memberEventLogs)
        {
            long reb = 0;

            if (memberEventLogs.Any())
            {
                //filter records to calculate points only 
                memberEventLogs = memberEventLogs.Where(x => x.IsFix != true && x.IsPause == 0 && x.IsShotClock != true &&
                                                         x.Stat.StatType.Name.ToLower() == Domain.Enums.Enum.StatType.Counter.ToString().ToLower() &&
                                                         x.Stat.Name.ToLower() == Domain.Enums.Enum.BasketballStat.REB.ToString().ToLower())
                                                         .ToList();
                //calculating sum of the filtered records
                reb = memberEventLogs.Count();
            }
            return reb;
        }

        /// <summary>
        /// calculate total steals from member
        /// </summary>
        /// <param name="memberEventLogs">Collection of logs for the member</param>
        /// <returns>steals</returns>
        private long CalculateSteals(List<EventLog> memberEventLogs)
        {
            long stls = 0;

            if (memberEventLogs.Any())
            {
                //filter records to calculate points only  
                memberEventLogs = memberEventLogs.Where(x => x.IsFix != true && x.IsPause == 0 && x.IsShotClock != true &&
                                                         x.Stat.StatType.Name.ToLower() == Domain.Enums.Enum.StatType.Counter.ToString().ToLower() &&
                                                         x.Stat.Name.ToLower() == Domain.Enums.Enum.BasketballStat.STLS.ToString().ToLower())
                                                         .ToList();
                //calculating sum of the filtered records
                stls = memberEventLogs.Count();
            }
            return stls;
        }

        /// <summary>
        /// calculate total blocks from member
        /// </summary>
        /// <param name="memberEventLogs">Collection of logs for the member</param>
        /// <returns>blocks</returns>
        private long CalculateBlocks(List<EventLog> memberEventLogs)
        {
            long blks = 0;

            if (memberEventLogs.Any())
            {
                //filter records to calculate points only 
                memberEventLogs = memberEventLogs.Where(x => x.IsFix != true && x.IsPause == 0 && x.IsShotClock != true &&
                                                         x.Stat.StatType.Name.ToLower() == Domain.Enums.Enum.StatType.Counter.ToString().ToLower() &&
                                                         x.Stat.Name.ToLower() == Domain.Enums.Enum.BasketballStat.BLKS.ToString().ToLower())
                                                         .ToList();
                //calculating sum of the filtered records
                blks = memberEventLogs.Count();
            }
            return blks;
        }

        /// <summary>
        /// calculate mvp percentage for member
        /// </summary>
        /// <param name="memberEventLogs">Collection of logs for the member</param>
        /// <returns>mvp</returns>
        private decimal CalculateMvp(List<EventLog> memberEventLogs, EventTeamMemberStatResult eventTeamMemberStatResult, EventTeam eventTeam)
        {
            decimal mvp = default!;
            string currentMVPFormula = MvpFormula;
            decimal totalPointForMvp = CalculatePointsForMvp(memberEventLogs);
            currentMVPFormula = currentMVPFormula.Replace("Points", totalPointForMvp.ToString());
            currentMVPFormula = currentMVPFormula.Replace("Assists", eventTeamMemberStatResult.Assist.ToString());
            currentMVPFormula = currentMVPFormula.Replace("Rebounds", eventTeamMemberStatResult.Rebound.ToString());
            currentMVPFormula = currentMVPFormula.Replace("Steals", eventTeamMemberStatResult.Steal.ToString());
            currentMVPFormula = currentMVPFormula.Replace("Blocks", eventTeamMemberStatResult.Block.ToString());

            if (eventTeam.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Win.ToString().ToLower() && eventTeam.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower())
            {
                currentMVPFormula = currentMVPFormula.Replace("TotalTeamPt", eventTeam.Points.ToString() + "*" + "0");
            }
            else
            {
                currentMVPFormula = currentMVPFormula.Replace("TotalTeamPt", eventTeam.Points.ToString() + "*" + "1");
            }

            mvp = CalculateFormula(currentMVPFormula);

            return mvp;
        }
        
        private decimal CalculateMvpForLive(List<EventLog> memberEventLogs, BasketballPlayerMatchSummary eventTeamMemberStatResult, EventTeam eventTeam)
        {
            decimal mvp = default!;
            string currentMVPFormula = MvpFormula;
            decimal totalPointForMvp = CalculatePointsForMvp(memberEventLogs);
            currentMVPFormula = currentMVPFormula.Replace("Points", totalPointForMvp.ToString());
            currentMVPFormula = currentMVPFormula.Replace("Assists", eventTeamMemberStatResult.Ast.ToString());
            currentMVPFormula = currentMVPFormula.Replace("Rebounds", eventTeamMemberStatResult.Rebs.ToString());
            currentMVPFormula = currentMVPFormula.Replace("Steals", eventTeamMemberStatResult.Stls.ToString());
            currentMVPFormula = currentMVPFormula.Replace("Blocks", eventTeamMemberStatResult.Blks.ToString());

            if (eventTeam.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Win.ToString().ToLower() && eventTeam.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower())
            {
                currentMVPFormula = currentMVPFormula.Replace("TotalTeamPt", eventTeam.Points.ToString() + "*" + "0");
            }
            else
            {
                currentMVPFormula = currentMVPFormula.Replace("TotalTeamPt", eventTeam.Points.ToString() + "*" + "1");
            }

            mvp = CalculateFormula(currentMVPFormula);

            return mvp;
        }

        /// <summary>
        /// method calculates total points for each team member just for mvp calculation
        /// </summary>
        /// <param name="memberEventLogs">member/player event stats</param>
        /// <returns>total points scored by player/member for mvp calculation</returns>
        private decimal CalculatePointsForMvp(List<EventLog> memberEventLogs)
        {
            decimal points = default!;

            if (memberEventLogs.Any())
            {

                //filter records to calculate points only 
                memberEventLogs = memberEventLogs.Where(x => x.IsFix != true && x.IsSucessfull != false && x.IsPause == 0 && x.IsShotClock != true &&
                                                             x.Stat.StatType.Name.ToLower() == Domain.Enums.Enum.StatType.HitMiss.ToString().ToLower())
                                                             .ToList();
                //calculating sum of the filtered records
                var groupBasedOnStatList = memberEventLogs.GroupBy(x => x.StatId).ToList();

                foreach (IGrouping<long, EventLog> groupBasedOnStat in groupBasedOnStatList)
                {
                    var statCount = groupBasedOnStat.Count();
                    var multiplyRes = statCount * groupBasedOnStat.First().Stat.MvpValue;

                    points += multiplyRes;
                }
            }

            return points;
        }

        /// <summary>
        /// calculate the formula
        /// </summary>
        /// <param name="input">takes the string formula with values</param>
        /// <returns>calculated results</returns>
        private decimal CalculateFormula(string input)
        {
            var res = input.Split("+");

            decimal result = 0;
            for (int i = 0; i < res.Length; i++)
            {
                decimal finalres = 1;
                var multiSplit = res[i].Split('*');
                for (int j = 0; j < multiSplit.Length; j++)
                {
                    finalres *= decimal.Parse(multiSplit[j]);
                }
                result += finalres;
            }
            return result;
        }
        /// <summary>
        /// add values in win score formula
        /// </summary>
        /// <param name="totalPoints">total points of player</param>
        /// <param name="eventTeam">event and team details object</param>
        /// <returns>win score for the player</returns>
        private decimal CalculateWinScoreFormula(decimal totalPoints, EventTeam eventTeam)
        {
            decimal winScore = 0;
            string currentScoreFormula = string.Empty;

            if (eventTeam.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Win.ToString().ToLower() && eventTeam.EventStatus.Name.ToLower() != Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower())
            {
                winScore = 0;
            }
            else
            {
                currentScoreFormula = WinScoreFormula;
                currentScoreFormula = currentScoreFormula.Replace("TotalPoints", totalPoints.ToString());
                winScore = CalculateWin(currentScoreFormula);
            }

            return winScore;
        }

        /// <summary>
        /// break the input string by '/' sign to divide to entries
        /// </summary>
        /// <param name="input">formula with actual values</param>
        /// <returns>actual win score percentage result</returns>
        private decimal CalculateWin(string input)
        {
            var divideSplit = input.Split("/");
            decimal result = 0;
            if (decimal.Parse(divideSplit[1]) != 0)
            {
                result = decimal.Parse(divideSplit[0]) / decimal.Parse(divideSplit[1]);
            }
            return result;
        }
        /// <summary>
        /// add actual value in mvp percentage formula
        /// </summary>
        /// <param name="numberOfTimeMvp">number of time player become as mvp</param>
        /// <param name="numberOfGamesPlayed"></param>
        /// <returns></returns>
        private decimal CalculateMvpPercentage(int numberOfTimeMvp, int numberOfGamesPlayed)
        {
            decimal mvpPercentage = 0;

            string MvpFormula = MvpPercentage;

            MvpFormula = MvpFormula.Replace("# of times as mvp", numberOfTimeMvp.ToString());
            MvpFormula = MvpFormula.Replace("# of games played", numberOfGamesPlayed.ToString());

            mvpPercentage = CalculateWin(MvpFormula);

            return mvpPercentage;
        }

        private decimal CalculateWinPercentage(int numberOfGamesWon, int numberOfGamesPlayed)
        {
            decimal WinPerce = 0;

            string WinFormula = WinPercentage;

            WinFormula = WinFormula.Replace("# of games won", numberOfGamesWon.ToString());
            WinFormula = WinFormula.Replace("# of games played", numberOfGamesPlayed.ToString());

            WinPerce = CalculateWin(WinFormula);

            return WinPerce;
        }

        public List<EventTeam> UpdateEventTeamPoint(List<EventLog> eventLogList, List<EventTeam> eventTeamList, List<TeamMember> teamMemberList, List<Domain.Entities.EventStatus> eventStatusList)
        {
            decimal maxPoint = default!;

            Domain.Entities.EventStatus winStatus = eventStatusList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventStatus.Win.ToString().ToLower()).FirstOrDefault();
            Domain.Entities.EventStatus loseStatus = eventStatusList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventStatus.Lose.ToString().ToLower()).FirstOrDefault();
            Domain.Entities.EventStatus tieStatus = eventStatusList.Where(x => x.Name.ToLower() == Domain.Enums.Enum.EventStatus.Tie.ToString().ToLower()).FirstOrDefault();

            foreach (EventTeam eventTeam in eventTeamList)
            {
                var teamMemberListForThisTeam = teamMemberList.Where(x => x.TeamId == eventTeam.EventCollectionTeam.TeamId).Select(x => x.Id).ToList();

                eventTeam.Points = eventLogList.Where(x => teamMemberListForThisTeam.Contains(x.TeamMemberId) &&
                                                           x.IsFix != true && x.IsSucessfull != false && x.IsPause == 0 && x.IsShotClock != true &&
                                                           x.Stat.StatType.Name.ToLower() == Domain.Enums.Enum.StatType.HitMiss.ToString().ToLower())
                                               .Select(x => x.Stat.Value).Sum();
                maxPoint = eventTeam.Points;
            }

            foreach (EventTeam eventTeam in eventTeamList)
            {
                if (maxPoint < eventTeam.Points)
                {
                    maxPoint = eventTeam.Points;
                }
            }

            var eventTeamWin = eventTeamList.Where(x => x.Points == maxPoint).ToList();
            if (eventTeamWin.Count > 1)
            {
                foreach (EventTeam eventTeam in eventTeamWin)
                {
                    eventTeam.EventStatusId = tieStatus.Id;
                }
            }
            else
            {
                eventTeamWin.FirstOrDefault().EventStatusId = winStatus.Id;
                var eventTeamLose = eventTeamList.Where(x => x.Points != maxPoint).FirstOrDefault();
                eventTeamLose.EventStatusId = loseStatus.Id;
            }

            return eventTeamList;
        }
        public List<BranchesVenuesResponse> StatusMapper(BasicFilter paginationFilter,List<BranchesVenuesResponseTemp> AllMatches, List<TeamDetail> Teams,List<Icon> ActivityId, List<Icon> StatkeeperIcons,string Role)
        {
            List<BranchesVenuesResponse> BranchesVenuesResponse = new List<BranchesVenuesResponse>();

            foreach (BranchesVenuesResponseTemp x in AllMatches)
            {
                if (Role != Domain.Enums.Enum.Roles.Player.ToString())
                {
                    if (x.Status == Domain.Enums.Enum.EventStatus.Win.ToString() ||
                       x.Status == Domain.Enums.Enum.EventStatus.Lose.ToString() ||
                       x.Status == Domain.Enums.Enum.EventStatus.Tie.ToString())
                    {
                        x.Status = Domain.Enums.Enum.EventStatus.Completed.ToString();
                    }
                }
                List<TeamDetail> eventTeams = Teams.Where(t => t.eventid == x.Id).ToList();


                List<Icon> ActivityImages = ActivityId.Where(t => t.Id == x.activityId).ToList();
                var ActivityImagesPath = ActivityImages.Select(t => t.Path).FirstOrDefault();
                List<Icon> StatkeeperIcon = StatkeeperIcons.Where(t => t.Id == x.StatkeeperId).ToList();
                var StatkeeperIconPath = StatkeeperIcons.Select(t => t.Path).FirstOrDefault();

                BranchesVenuesResponse response = new BranchesVenuesResponse();
                response.Teams = new List<TeamDetail>();


                response.Id = x.Id;
                response.venueId = x.venueId;
                response.EventType = x.EventType;
                response.Status = x.Status;
                response.VenueName = x.VenueName;
                response.BranchName = x.BranchName;
                response.EventDuration = x.EventDuration;
                response.StartTime = x.StartTime;
                response.EndTime = x.EndTime;
                response.EventDate = x.EventDate;
                response.StatskeeperName = x.StatskeeperName;
                response.CurrentActivity = x.CurrentActivity;
                response.ActivityIcon = !string.IsNullOrEmpty(ActivityImagesPath) ? ActivityImagesPath : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
                response.StatkeeperImage = !string.IsNullOrEmpty(StatkeeperIconPath) ? StatkeeperIconPath : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;

                response.Teams.AddRange(eventTeams);
                BranchesVenuesResponse.Add(response);
            }

            var Branches = BranchesVenuesResponse.Where(x => !string.IsNullOrEmpty(paginationFilter.sSearch) ?
                                                        x.Status.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) ||
                                                        x.BranchName.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) ||
                                                        x.VenueName.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) ||
                                                        x.EventType.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) ||
                                                        x.CurrentActivity.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) :
                                                        x.Id != 0)
                                                        //.OrderByDescending(x => x.Id)
                                                        //.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                                                        //.Take(paginationFilter.PageSize)
                                                        .ToList();
                                         
            return Branches;
        }
        public  int StatusMapperCount(BasicFilter paginationFilter, List<BranchesVenuesResponseTemp> AllMatches, List<TeamDetail> Teams, List<Icon> ActivityId, List<Icon> StatkeeperIcons, string Role)
        {
            List<BranchesVenuesResponse> BranchesVenuesResponse = new List<BranchesVenuesResponse>();

            foreach (BranchesVenuesResponseTemp x in AllMatches)
            {
                if (Role != Domain.Enums.Enum.Roles.Player.ToString())
                {
                    if (x.Status == Domain.Enums.Enum.EventStatus.Win.ToString() ||
                       x.Status == Domain.Enums.Enum.EventStatus.Lose.ToString() ||
                       x.Status == Domain.Enums.Enum.EventStatus.Tie.ToString())
                    {
                        x.Status = Domain.Enums.Enum.EventStatus.Completed.ToString();
                    }
                }
                List<TeamDetail> eventTeams = Teams.Where(t => t.eventid == x.Id).ToList();


                List<Icon> ActivityImages = ActivityId.Where(t => t.Id == x.activityId).ToList();
                var ActivityImagesPath = ActivityImages.Select(t => t.Path).FirstOrDefault();
                List<Icon> StatkeeperIcon = StatkeeperIcons.Where(t => t.Id == x.StatkeeperId).ToList();
                var StatkeeperIconPath = StatkeeperIcons.Select(t => t.Path).FirstOrDefault();

                BranchesVenuesResponse response = new BranchesVenuesResponse();
                response.Teams = new List<TeamDetail>();


                response.Id = x.Id;
                response.venueId = x.venueId;
                response.EventType = x.EventType;
                response.Status = x.Status;
                response.VenueName = x.VenueName;
                response.BranchName = x.BranchName;
                response.EventDuration = x.EventDuration;
                response.StartTime = x.StartTime;
                response.EndTime = x.EndTime;
                response.EventDate = x.EventDate;
                response.StatskeeperName = x.StatskeeperName;
                response.CurrentActivity = x.CurrentActivity;
                response.ActivityIcon = !string.IsNullOrEmpty(ActivityImagesPath) ? ActivityImagesPath : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
                response.StatkeeperImage = !string.IsNullOrEmpty(StatkeeperIconPath) ? StatkeeperIconPath : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;

                response.Teams.AddRange(eventTeams);
                BranchesVenuesResponse.Add(response);
            }

            var Branches = BranchesVenuesResponse.Where(x => !string.IsNullOrEmpty(paginationFilter.sSearch) ?
                                                        x.Status.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) ||
                                                        x.BranchName.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) ||
                                                        x.VenueName.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) ||
                                                        x.EventType.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) ||
                                                        x.CurrentActivity.ToLower().Contains(paginationFilter.sSearch.ToLower().Trim()) :
                                                        x.Id != 0).Count();
                                         

            return Branches;
        }
        public Domain.Entities.SubstitutePlayer SubstituteMapper(long teamId, long eventId, long substituteId, long substituteToId,string email,bool isDisqaulify)
        {
            Domain.Entities.SubstitutePlayer response = new Domain.Entities.SubstitutePlayer();

            response.EventId = eventId;
            response.TeamMemberId = teamId;
            response.SubstitutedPlayer = substituteId;
            response.SubstituteToPlayer = substituteToId;
            response.IsDisqualify = true;
            response.Description = "Player has been Disable";
            response.CreatedBy = email;
            response.LastModifiedBy = email;

            return response;


        }

        public MatchSummaryResp MapMatchSummary(BasicFilter paginationFilter,DateTime eventDateTime, List<MatchSummaryRespList> MatchSummaryRespList, long Quarters)
        {
            MatchSummaryResp matchSummaryResp = new MatchSummaryResp();
            var PaginationMatchSummary =  MatchSummaryRespList.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                     .Take(paginationFilter.PageSize)
                     .ToList();
            matchSummaryResp.eventStartDateTime = eventDateTime;
            matchSummaryResp.Quarters = Quarters;
            matchSummaryResp.MatchSummaryRespLists = new List<MatchSummaryRespList>();
            foreach (MatchSummaryRespList resp in PaginationMatchSummary)
            {
                matchSummaryResp.MatchSummaryRespLists.Add(resp);
            }
            return matchSummaryResp;


        }

        public EventDurationModel CalculateEventPausedTimeDurationInMinutes(List<EventLog> eventLogList,DateTime eventStartDateTime,int activityDurationInMinutes,bool isEventEnd,DateTime eventEndDateTime,bool isEventStart)
        {
            EventDurationModel eventDurationModel = new EventDurationModel();
            eventDurationModel.eventDurationMinute = 0;
            eventDurationModel.eventDurationSecond = 0;

            var checkIfMatchIsPaused = eventLogList.Where(x => x.IsPause != 0).ToList();

            var numberOfPause = checkIfMatchIsPaused.Where(x => x.IsPause == 1).Count();
            var numberOfResume = checkIfMatchIsPaused.Where(x => x.IsPause == 2).Count();
            if (!isEventStart)
            {
                return eventDurationModel;
            }
            else if (eventLogList.Any() && numberOfPause != numberOfResume)
            {
                var lastLogOfEvent = eventLogList.LastOrDefault();
                var lastLogDateTime = lastLogOfEvent.CreatedDateTime;

                var removeLastPause = checkIfMatchIsPaused.Take(checkIfMatchIsPaused.Count() - 1).ToList();

                TimeSpan pausedDuration = CalculatePausedDuration(removeLastPause);

                var checkCase = lastLogDateTime.Subtract(eventStartDateTime);
                checkCase = checkCase.Subtract(pausedDuration);
                eventDurationModel.eventDurationMinute = checkCase.Minutes;
                eventDurationModel.eventDurationSecond = checkCase.Seconds;
            }
            else
            {
                var currentTime = isEventEnd != true ? DateTime.UtcNow : eventEndDateTime;
                TimeSpan pausedDuration = CalculatePausedDuration(checkIfMatchIsPaused);

                var checkCase = currentTime.Subtract(eventStartDateTime);
                checkCase = checkCase.Subtract(pausedDuration);
                eventDurationModel.eventDurationMinute = checkCase.Minutes;
                eventDurationModel.eventDurationSecond = checkCase.Seconds;
            }
       
            return eventDurationModel;
        }


        public TimeSpan CalculatePausedDuration(List<EventLog> eventLogList)
        {
            TimeSpan pausedDuration = default;

            for (int i = eventLogList.Count - 1; i >= 0; i--)
            {
                var resumeDateTime = eventLogList[i].CreatedDateTime;
                var pausedDateTime = eventLogList[i - 1].CreatedDateTime;

                pausedDuration += resumeDateTime.Subtract(pausedDateTime);

          
                i--;
            }

            return pausedDuration;
        }
    }
}
