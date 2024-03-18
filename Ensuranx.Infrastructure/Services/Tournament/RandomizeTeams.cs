using Ensuranx.Application.Response.Tournament;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.ExtensionMethods;
using Microsoft.Extensions.Logging;

namespace Ensuranx.Infrastructure.Services.Tournament
{
    /// <summary>
    /// randomize Team for events for tournament season
    /// </summary>
    public class RandomizeTeams
    {
        private readonly ILogger _logger;
        public RandomizeTeams(ILogger logger)
        {
            _logger = logger;
        }

        public Domain.Entities.Tournament Tournament { get; set; }
        public List<long> TeamIdList { get; set; }
        public List<VenuesStatskeeper> VenueStatskeeperList { get; set; }
        public List<Domain.Entities.Venue> TournamentVenueList { get; set; }
        public List<long> TournamentStatkeeperIdList { get; set; }
        public List<BranchScheduling> BranchSchedulingList { get; set; }

        public int EachGameDuration { get; set; } = 75;

        /// <summary>
        /// calculating number of matches for the season
        /// </summary>
        /// <returns></returns>
        private int CalculateNumberOfMatches()
        {
            int count = 0;
            int teamCount = this.TeamIdList.Count;

            count = teamCount * (teamCount - 1);
            count = count / 2;
            return count;
        }


        /// <summary>
        /// calculate the match count for a venue
        /// </summary>
        /// <returns></returns>
        private int VenueMatchCount(List<BranchScheduling> branchSchedulingList,long tournamentVenueCount)
        {
            int venueMatchCount = 0;

            // season start date saved to update it later
            var startDateOfSeason = this.Tournament.SeasonStartDate;
            // season end date saved to update it later
            var endDateOfSeason = this.Tournament.SeasonEndDate;

            while (startDateOfSeason <= endDateOfSeason)
            {
                for (int i = 0; i < tournamentVenueCount; i++)
                {
                    var day = startDateOfSeason.DayOfWeek;

                    //getting the schedule details for that particular day
                    var schedule = branchSchedulingList.Where(x => x.WeekDay.ToString() == day.ToString()).FirstOrDefault();

                    if (schedule is not null)
                    {
                        //getting start and end time for the venue at that day
                        var startTime = schedule.StartTime;
                        var endTime = schedule.EndTime;

                        //now calculating number of matches we can schedule
                        int numberOfGames = CalculateNumberOfGames(startTime, endTime, this.EachGameDuration);
                        venueMatchCount += numberOfGames;
                    }
                }
                startDateOfSeason = startDateOfSeason.Date.AddDays(1);
            }
            
            return venueMatchCount;
        }

        /// <summary>
        /// checking if their are enough space and time in the venues to schedule all matches
        /// </summary>
        /// <returns></returns>
        private bool CheckIfTheMatchesHaveRequiredSpaceAndTime()
        {
            bool check = true;

            long tournamentVenueCount = this.TournamentVenueList.Count;

            //calculating number of matches
            int totalNumberOfMatches = CalculateNumberOfMatches();
            var listOfScehdulingVenue = this.BranchSchedulingList;
            //calculating total matches can be played in all venues
            int canPlayMatchCount = VenueMatchCount(listOfScehdulingVenue, tournamentVenueCount);

            //checking if the total matches to be played are greater or equal to matches capacity in venues
            check = totalNumberOfMatches >= canPlayMatchCount ? false : true;

            return check;
        }

        /// <summary>
        /// checking all the validation before scheduling matches
        /// </summary>
        /// <returns></returns>
        public bool ValidationCheckForSchedule()
        {
            bool checkRes = true;

            if (!CheckIfTheMatchesHaveRequiredSpaceAndTime())
            {
                checkRes = false;
            }
            else 
            { 
                checkRes = true;
            }

            return checkRes;
        }

        /// <summary>
        /// schedule events between the teams
        /// </summary>
        /// <returns></returns>
        public List<RandomizeTeamResponse> ScheduleSeasonMatches()
        {
            List<RandomizeTeamResponse> randomizeTeamResponseList = new List<RandomizeTeamResponse>();

            randomizeTeamResponseList = GenerateMatchups(this.TeamIdList);
            randomizeTeamResponseList = AssignVenueToEvent(randomizeTeamResponseList,this.TournamentVenueList,this.TournamentStatkeeperIdList,this.VenueStatskeeperList, this.Tournament.SeasonStartDate,
                this.Tournament.SeasonEndDate);

            return randomizeTeamResponseList;
        }

        private List<RandomizeTeamResponse> AssignVenueToEvent(List<RandomizeTeamResponse> randomizeTeamResponseList, List<Domain.Entities.Venue> TournamentVenueList,List<long> statkeeperIdList
            , List<VenuesStatskeeper> VenueStatskeeperList,DateTime startDate,DateTime endDate)
        {
            // Shuffle the list randomly
            Random rand = new Random();

            //selecting venueId from tournament venue list
            List<long> venueIdList = TournamentVenueList.Select(x => x.Id).ToList();

            //shuffle matches list
            randomizeTeamResponseList.Shuffle();

            // total matches count
            int matchesCount = randomizeTeamResponseList.Count - 1;
            // season start date saved to update it later
            var startDateOfTournament = startDate;
            // season end date saved to update it later
            var endDateOfTournament = endDate;

            while (matchesCount != 0 && startDateOfTournament < endDateOfTournament)
            {
                for (int i = 0; i < venueIdList.Count; i++)
                {
                    // Choose a venue for the match
                    long venueId = venueIdList[i];

                    //getting all the available statkeeper id
                    var statkeeperIdListList = VenueStatskeeperList.Where(x => x.VenueId == venueId && statkeeperIdList.Contains(x.UserInfoId.Value)).Select(x => x.UserInfoId.Value).ToList();
                    if (statkeeperIdListList.Any())
                    {
                        //getting the venue details to pick branch id for schedule time details
                        var venueDetails = this.TournamentVenueList.Where(x => x.Id == venueId).FirstOrDefault();
                        //picking venue scheduling details 
                        var venueSchedule = this.BranchSchedulingList.Where(x => x.BranchId == venueDetails.BranchId).ToList();
                        //getting the day of the week from season start date
                        var day = startDateOfTournament.DayOfWeek;
                        //getting the schedule details for that particular day
                        var schedule = venueSchedule.Where(x => x.WeekDay.ToString() == day.ToString()).FirstOrDefault();

                        if (schedule is not null)
                        {
                            //getting start and end time for the venue at that day
                            var startTime = schedule.StartTime;
                            var endTime = schedule.EndTime;

                            //now calculating number of matches we can schedule
                            int numberOfGames = CalculateNumberOfGames(startTime, endTime, this.EachGameDuration);

                            for (int j = 0; j < numberOfGames; j++)
                            {
                                if (matchesCount != -1)
                                {
                                    //calculating date and time of the match
                                    var dateOfTheMatch = startDateOfTournament.Date + startTime;
                                    //getting match to schedule from matches list
                                    var matchToSchedule = randomizeTeamResponseList[matchesCount];
                                    //List<long> matchTeamIdList = matchToSchedule.EventTeamIdList.ToList();

                                    //bool check = CheckTeamAlreadyPlayedInTheGivenDate(randomizeTeamResponseList, matchTeamIdList, dateOfTheMatch);

                                    // Choose a random statkeeper for the match
                                    long statkeeperId = statkeeperIdListList[rand.Next(statkeeperIdListList.Count)];

                                    matchToSchedule.VenueId = venueId;
                                    matchToSchedule.StatkeeperId = statkeeperId;

                                    //checking if the 
                                    startTime = startTime + TimeSpan.FromMinutes(double.Parse(this.EachGameDuration.ToString()));
                                    matchToSchedule.EventDateTime = dateOfTheMatch;
                                    matchesCount--;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Skip Venue {venueId} because it have no statkeeper");
                        continue;
                    }
                }
                startDateOfTournament = startDateOfTournament.Date.AddDays(1);
                if (startDateOfTournament.Date >= endDateOfTournament.Date)
                {
                    break;
                }

            }

            return randomizeTeamResponseList;
        }

        public bool CheckTeamAlreadyPlayedInTheGivenDate(List<RandomizeTeamResponse> randomizeTeamResponseList, List<long> teamIdList,DateTime dateOfTheMatch)
        {
            bool canSchedule = true;

            var getThisDateMatchTeam = randomizeTeamResponseList.Where(x => x.EventDateTime.Date == dateOfTheMatch.Date && x.EventTeamIdList.Any(item => teamIdList.Contains(item))).ToList();
            //List<long> thoseWhichHaveMatchScheduleForThisDate = new List<long>();
            //thoseWhichHaveMatchScheduleForThisDate.AddRange(getThisDateMatchTeam);

            canSchedule = getThisDateMatchTeam.Any() ? false : true;

            return canSchedule;
        }

        public int CalculateNumberOfGames(TimeSpan startTime, TimeSpan endTime, int gameDuration)
        {
            TimeSpan availableTime = endTime - startTime;
            int numberOfGames = (int)(availableTime.TotalMinutes / gameDuration);
            return numberOfGames;
        }

        private List<RandomizeTeamResponse> GenerateMatchups(List<long> teams)
        {
            List<RandomizeTeamResponse> randomizeTeamResponseList = new List<RandomizeTeamResponse>();

            for (int i = 0; i < teams.Count; i++)
            {
                for (int j = i + 1; j < teams.Count; j++)
                {
                    RandomizeTeamResponse randomizeTeamResponse = new RandomizeTeamResponse();
                    List<long> matches = new List<long>();

                    matches.Add(teams[i]);
                    matches.Add(teams[j]);

                    randomizeTeamResponse.EventTeamIdList = new List<long>();
                    randomizeTeamResponse.EventTeamIdList.AddRange(matches);

                    randomizeTeamResponseList.Add(randomizeTeamResponse);
                }
            }

            return randomizeTeamResponseList;
        }
    }
}

