using Ensuranx.Application.Requests.Team;
using Ensuranx.Application.Response.Event;
using Ensuranx.Application.Response.Team;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Common.Constant;
using Ensuranx.Domain.Entities;
using System.Linq;

namespace Ensuranx.Infrastructure.Mappers
{
    public class TeamMapper
    {
        private string getRandColor()
        {
            Random rnd = new Random();
            string hexOutput = String.Format("{0:X}", rnd.Next(0, 0xFFFFFF));
            while (hexOutput.Length < 6)
                hexOutput = "0" + hexOutput;
            return "#" + hexOutput;
        }

        public Domain.Entities.Team MapTeam(long teamTypeId, string name, string email)
        {
            Domain.Entities.Team team = new Domain.Entities.Team();

            var colorCode = getRandColor();

            team.Name = name;
            team.TeamTypeId = teamTypeId;
            team.CreatedDateTime = DateTime.UtcNow;
            team.LastModifiedDateTime = DateTime.UtcNow;
            team.CreatedBy = email;
            team.LastModifiedBy = email;
            team.Colour = colorCode;

            return team;
        }

        public TeamActivity MapTeamActivity(long teamId, long activityId, string email)
        {
            TeamActivity teamActivity = new TeamActivity();

            teamActivity.TeamId = teamId;
            teamActivity.ActivityId = activityId;
            teamActivity.CreatedDateTime = DateTime.UtcNow;
            teamActivity.LastModifiedDateTime = DateTime.UtcNow;
            teamActivity.CreatedBy = email;
            teamActivity.LastModifiedBy = email;

            return teamActivity;
        }

        public TeamMember MapTeamMemberWithTeam(long teamId, string email, long teamMemberRoleId, long playerId)
        {
            TeamMember teamMember = new TeamMember();

            teamMember.TeamId = teamId;
            teamMember.TeamMemberRoleId = teamMemberRoleId;
            teamMember.PlayerId = playerId;
            teamMember.CreatedDateTime = DateTime.UtcNow;
            teamMember.LastModifiedDateTime = DateTime.UtcNow;
            teamMember.CreatedBy = email;
            teamMember.LastModifiedBy = email;
            teamMember.IsAccepted = true;
            teamMember.IsBan = false;

            return teamMember;
        }
        public Notifications MapTeamMemberWithNotification(long playerId, string email, long userId)
        {
            Notifications Notif = new Notifications();

            Notif.NotificationsType = Domain.Enums.Enum.NotificationsTypes.TeamJoinRequest;
       
            //Notif. = teamMemberRoleId;
          //  Notif.PlayerId = playerId;
            Notif.CreatedDateTime = DateTime.UtcNow;
            Notif.LastModifiedDateTime = DateTime.UtcNow;
            Notif.CreatedBy = email;
            Notif.LastModifiedBy = email;
            Notif.IsActive = true;
            Notif.IsDeleted = false;

            return Notif;
        }

        public GetTeamPlayersStat MapTeamMembersStatsByTeam(Team team, List<TeamMember> teamMemberList, List<EventTeamMemberStatResult> eventTeamMemberStatResultList,
            int win, int lose, int tie, List<Images> memberImagesList, List<TeamMemberMatchesWon> teamMemberMatchesWonList, DateTime lastPlayedDateTime)
        {
            GetTeamPlayersStat getTeamPlayersStat = new GetTeamPlayersStat();

            getTeamPlayersStat.TeamId = team.Id;
            getTeamPlayersStat.TeamName = team.Name;
            getTeamPlayersStat.Colour = team.Colour;
            getTeamPlayersStat.DateCreated = team.CreatedDateTime;
            getTeamPlayersStat.TeamType = team.TeamType.Name;
            getTeamPlayersStat.Won = win;
            getTeamPlayersStat.Loss = lose;
            getTeamPlayersStat.Tie = tie;
            getTeamPlayersStat.Match = win + lose + tie;
            getTeamPlayersStat.PlayersCount = teamMemberList.Count;
            getTeamPlayersStat.LastPlayedDate = lastPlayedDateTime;
            getTeamPlayersStat.BasketballPlayerMatchSummaryList = new List<Application.Response.Event.BasketballPlayerMatchSummary>();

            foreach (TeamMember member in teamMemberList)
            {
                BasketballPlayerMatchSummary basketballPlayerMatchSummary = new BasketballPlayerMatchSummary();

                //getting all the previous matches records for the player
                List<EventTeamMemberStatResult> currentMemberEventsResults = eventTeamMemberStatResultList.Any() ?
                    eventTeamMemberStatResultList.Where(x => x.TeamMemberId == member.Id).ToList() : new List<EventTeamMemberStatResult>();

                Images userProfileImage = memberImagesList.Where(x => x.TableId == member.PlayerId.Value).FirstOrDefault();

                //matches won count for this player
                long CountMatchesWon = teamMemberMatchesWonList.Where(x => x.TeamMemberId == member.PlayerId).Select(x => x.MatchesWonCount).FirstOrDefault();

                basketballPlayerMatchSummary.Id = member.PlayerId.Value;
                basketballPlayerMatchSummary.PlayerName = member.UserInfo.Name;
                basketballPlayerMatchSummary.PlayerRole = member.TeamMemberRole.Name;

                basketballPlayerMatchSummary.Pts = currentMemberEventsResults.Any() ? currentMemberEventsResults.Select(x => x.TotalPoint).Sum() : 0;
                basketballPlayerMatchSummary.Ast = currentMemberEventsResults.Any() ? currentMemberEventsResults.Select(x => x.Assist).Sum() : 0;
                basketballPlayerMatchSummary.Rebs = currentMemberEventsResults.Any() ? currentMemberEventsResults.Select(x => x.Rebound).Sum() : 0;
                basketballPlayerMatchSummary.Stls = currentMemberEventsResults.Any() ? currentMemberEventsResults.Select(x => x.Steal).Sum() : 0;
                basketballPlayerMatchSummary.Blks = currentMemberEventsResults.Any() ? currentMemberEventsResults.Select(x => x.Block).Sum() : 0;

                basketballPlayerMatchSummary.Gp = currentMemberEventsResults.Select(x => x.EventId).Distinct().Count();
                basketballPlayerMatchSummary.MvpCount = currentMemberEventsResults.Where(x => x.IsMvp != false).Select(x => x.IsMvp).Count();

                var MvpPercentageCheck = Convert.ToDouble(basketballPlayerMatchSummary.MvpCount) / Convert.ToDouble(basketballPlayerMatchSummary.Gp);
                basketballPlayerMatchSummary.MvpPercentage = string.Concat(MvpPercentageCheck.ToString(), "%");

                double WinPercentageCheck = Convert.ToDouble(CountMatchesWon) / Convert.ToDouble(basketballPlayerMatchSummary.Gp);
                basketballPlayerMatchSummary.WinPercentage = string.Concat(WinPercentageCheck.ToString(), "%");

                basketballPlayerMatchSummary.WinScore = currentMemberEventsResults.Any() ? currentMemberEventsResults.Select(x => x.WinScore).Sum() : 0;
                basketballPlayerMatchSummary.TotalScore = currentMemberEventsResults.Any() ? currentMemberEventsResults.Select(x => x.TotalScore).Sum() : 0;
                basketballPlayerMatchSummary.ProfileImage = userProfileImage != null ? userProfileImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;

                getTeamPlayersStat.BasketballPlayerMatchSummaryList.Add(basketballPlayerMatchSummary);
            }

            return getTeamPlayersStat;
        }

        public Team MapTeamForUpdate(Team team, UpdateTeam updateTeam, string email)
        {
            team.Name = !string.IsNullOrEmpty(updateTeam.Name) ? updateTeam.Name : team.Name;
            team.LastModifiedBy = email;
            team.LastModifiedDateTime = DateTime.UtcNow;

            return team;
        }

        public TeamActivity MapTeamActivityForUpdate(TeamActivity teamActivity, UpdateTeam updateTeam, string email)
        {
            teamActivity.ActivityId = updateTeam.ActivityId != 0 ? updateTeam.ActivityId : teamActivity.ActivityId;
            teamActivity.LastModifiedBy = email;
            teamActivity.LastModifiedDateTime = DateTime.UtcNow;

            return teamActivity;
        }

        /// <summary>
        /// increment string by one
        /// </summary>
        /// <param name="sequenceIncrement"></param>
        /// <returns></returns>
        public int FindThePlayerNumberSequence(List<WaitingList> waitingList, long playerId)
        {
            for (int i = 0; i < waitingList.Count; i++)
            {
                if (waitingList[i].UserInfoId == playerId)
                {
                    return i + 1;
                }
            }
            return 0;
        }

        public List<GetTeamDetailsByEventCollectionRes> GetTeamsByEventCollectionId(List<GetTeamDetailsByEventCollectionRes> teams, List<PlayerList> players,
            List<WaitingList> waitingList,long eventId)
        {
            List<GetTeamDetailsByEventCollectionRes> GetTeamDetailsByEventCollectionRes = new List<GetTeamDetailsByEventCollectionRes>();
            foreach (GetTeamDetailsByEventCollectionRes x in teams)
            {
                GetTeamDetailsByEventCollectionRes response = new GetTeamDetailsByEventCollectionRes();
                List<PlayerList> playerList = players.Where(t => t.teamId == x.teamId).ToList();

                response.playerLists = new List<PlayerList>();

                foreach (var player in playerList)
                {
                    int waitingListNumber = waitingList.Where(x => x.UserInfoId == player.playerId.Value).Select(x => x.PlayerNumber).FirstOrDefault();
                    string waitingListNumberSeq = waitingListNumber < 10 ? "0" + waitingListNumber.ToString() : waitingListNumber.ToString();
                    player.sequence = eventId != 0 ? player.sequence != "0" ? player.sequence : waitingListNumberSeq : waitingListNumberSeq;
                }

                response.teamId = x.teamId;
                response.teamName = x.teamName;
                response.Colour = x.Colour;
                response.InProgress = x.InProgress;
                response.WinCount = x.WinCount;
                response.IsFilled = x.IsFilled;
                response.IsJoined = x.IsJoined;

                response.playerLists.AddRange(playerList);

                GetTeamDetailsByEventCollectionRes.Add(response);

            }

            return GetTeamDetailsByEventCollectionRes;
        }

        public GetAllMembersListInvite MapInvitedMemberListAndRecentlyPlayer(List<TeamMember> invitedNotAcceptedList, List<Follow> followList,
            List<TeamMember> recentlyPlayedWithMatchPlayerList, long loginUserId, List<Images> profileImageList, string sSearch)
        {
            GetAllMembersListInvite getAllMembersListInvite = new GetAllMembersListInvite();

            getAllMembersListInvite.Invited = new List<GetInviteMemberList>();
            getAllMembersListInvite.Friends = new List<GetInviteMemberList>();
            getAllMembersListInvite.RecentlyPlayedWith = new List<GetInviteMemberList>();

            for (int i = 0; i < invitedNotAcceptedList.Count; i++)
            {
                GetInviteMemberList getInviteMemberList = new GetInviteMemberList();

                var selectImage = profileImageList.Where(x => x.TableId == invitedNotAcceptedList[i].PlayerId.Value).FirstOrDefault();

                getInviteMemberList.Id = invitedNotAcceptedList[i].PlayerId.Value;
                getInviteMemberList.Name = invitedNotAcceptedList[i].UserInfo.Name;
                getInviteMemberList.Path = selectImage != null ? selectImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;

                getAllMembersListInvite.Invited.Add(getInviteMemberList);
                if (!string.IsNullOrEmpty(sSearch))
                {
                    getAllMembersListInvite.Invited = getAllMembersListInvite.Invited.Where(x => x.Name.ToLower().Contains(sSearch.ToLower())).ToList();
                }
            }

            for (int i = 0; i < followList.Count; i++)
            {
                GetInviteMemberList getInviteMemberList = new GetInviteMemberList();

                if (followList[i].FollowerId.Value != loginUserId)
                {
                    var selectImage = profileImageList.Where(x => x.TableId == followList[i].FollowerId.Value).FirstOrDefault();

                    getInviteMemberList.Id = followList[i].FollowerId.Value;
                    getInviteMemberList.Name = followList[i].UserInfo.Name;
                    getInviteMemberList.Path = selectImage != null ? selectImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
                }
                else
                {
                    var selectImage = profileImageList.Where(x => x.TableId == followList[i].FolloweeId.Value).FirstOrDefault();

                    getInviteMemberList.Id = followList[i].FolloweeId.Value;
                    getInviteMemberList.Name = followList[i].UserInfo2.Name;
                    getInviteMemberList.Path = selectImage != null ? selectImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;
                }

                getAllMembersListInvite.Friends.Add(getInviteMemberList);
                if (!string.IsNullOrEmpty(sSearch))
                {
                    getAllMembersListInvite.Friends = getAllMembersListInvite.Friends.Where(x => x.Name.ToLower().Contains(sSearch.ToLower())).ToList();
                }
            }

            for (int i = 0; i < recentlyPlayedWithMatchPlayerList.Count; i++)
            {
                GetInviteMemberList getInviteMemberList = new GetInviteMemberList();

                var selectImage = profileImageList.Where(x => x.TableId == recentlyPlayedWithMatchPlayerList[i].PlayerId.Value).FirstOrDefault();

                getInviteMemberList.Id = recentlyPlayedWithMatchPlayerList[i].PlayerId.Value;
                getInviteMemberList.Name = recentlyPlayedWithMatchPlayerList[i].UserInfo.Name;
                getInviteMemberList.Path = selectImage != null ? selectImage.Path : Constants.APIErrorMessages.USER_DEFAULT_PROFILE_IMAGE;

                getAllMembersListInvite.RecentlyPlayedWith.Add(getInviteMemberList);
                if (!string.IsNullOrEmpty(sSearch))
                {
                    getAllMembersListInvite.RecentlyPlayedWith = getAllMembersListInvite.RecentlyPlayedWith.Where(x => x.Name.ToLower().Contains(sSearch.ToLower())).ToList();
                }
            }

            return getAllMembersListInvite;
        }

        public GetAllAvgResp GetAvgMapper(List<GetAllAvgResp> getAllAvgResps)
        {
            GetAllAvgResp getAllAvgRespOut = new GetAllAvgResp();

                getAllAvgRespOut.AvgAssit = Math.Round(getAllAvgResps.Select(x => x.AvgAssit).Average(), 3);
                getAllAvgRespOut.AvgBlock = Math.Round(getAllAvgResps.Select(x => x.AvgBlock).Average(), 3);
                getAllAvgRespOut.AvgRebound = Math.Round(getAllAvgResps.Select(x => x.AvgRebound).Average(), 3);
                getAllAvgRespOut.AvgSteal = Math.Round(getAllAvgResps.Select(x => x.AvgSteal).Average(), 3);
                getAllAvgRespOut.AvgTotalPoint = Math.Round(getAllAvgResps.Select(x => x.AvgTotalPoint).Average(), 3);
                getAllAvgRespOut.AvgTotalScore = Math.Round(getAllAvgResps.Select(x => x.AvgTotalScore).Average(), 3);
                getAllAvgRespOut.AvgMvpPerc = Math.Round(getAllAvgResps.Select(x => x.AvgMvpPerc).Average(), 3);
                getAllAvgRespOut.AvgWinScore = Math.Round(getAllAvgResps.Select(x => x.AvgWinScore).Average(), 3);
                getAllAvgRespOut.AvgWinPerc =Math.Round(getAllAvgResps.Select(x => x.AvgWinPerc).Average(),3);
                getAllAvgRespOut.AvgStealWinPercent = Math.Round(getAllAvgResps.Select(x => x.AvgStealWinPercent).Average(),3);
               //getAllAvgRespOut.AvgAssit = Math.Round(getAllAvgResps.Select(x => x.AvgAssit).Average(), 2);

            return getAllAvgRespOut;
        }
    }
}
