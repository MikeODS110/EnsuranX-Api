using Ensuranx.Application.Requests.Tournament;
using Ensuranx.Application.Response.Tournament;
using Ensuranx.Application.Response.User;
using Ensuranx.Domain.Entities;
using Mapster;
using System.Linq;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Mappers
{
    public class TournamentMapper
    {
        public Tournament MapCreateTournament(CreateTournament createTournament, string email, Domain.Entities.TournamentType tournamentType)
        {
            Tournament tournament = new Tournament();
            DateTime dateTime = DateTime.UtcNow;
            TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

            var config = TypeAdapterConfig.GlobalSettings;
            TypeAdapterConfig<(CreateTournament createTournament, Domain.Entities.TournamentType tournamentType, string email, DateTime dateTime), Tournament>.NewConfig()
          //  TypeAdapterConfig<(CreateTournament createTournament, Domain.Entities.TournamentType tournamentType, string email, DateTime dateTime,DateTime StartDateTime,DateTime SeasonEndDate, int SeasonTeamCapacity), Tournament>.NewConfig()
    
            .Map(dest => dest.LastModifiedBy, src => src.email)
            .Map(dest => dest.CreatedBy, src => src.email)
            .Map(dest => dest.CreatedDateTime, src => src.dateTime)
            .Map(dest => dest.LastModifiedDateTime, src => src.dateTime)
            .Map(dest => dest.TournamentTypeId, src => src.tournamentType.Id)
            //.Map(dest => dest.SeasonStartDate, src => src.StartDateTime)
            //.Map(dest => dest.SeasonEndDate, src => src.SeasonEndDate)
            //.Map(dest => dest.SeasonTeamCapacity, src => src.SeasonTeamCapacity)
            .Map(dest => dest, src => src.createTournament);
            
            tournament = (createTournament, tournamentType, email, dateTime).Adapt<Tournament>();

            return tournament;
        }

        public List<TournamentVenue> MapCreateTournamentVenue(CreateTournament createTournament, string email, long tournamentId)
        {
            List<TournamentVenue> tournamentVenueList = new List<TournamentVenue>();

            for (int i = 0; i < createTournament.VenueIdList.Count; i++)
            {
                TournamentVenue tournamentVenue = new TournamentVenue();

                tournamentVenue.CreatedDateTime = DateTime.UtcNow;
                tournamentVenue.LastModifiedDateTime = DateTime.UtcNow;
                tournamentVenue.CreatedBy = email;
                tournamentVenue.LastModifiedBy = email;
                tournamentVenue.TournamentId = tournamentId;
                tournamentVenue.VenueId = createTournament.VenueIdList[i];
                tournamentVenueList.Add(tournamentVenue);
            }
            return tournamentVenueList;
        }

        public List<TournamentStatkeeper> MapCreateTournamentStatkeeper(CreateTournament createTournament, string email, long tournamentId)
        {
            List<TournamentStatkeeper> tournamentStatkeeperList = new List<TournamentStatkeeper>();

            for (int i = 0; i < createTournament.StatkeeperIdList.Count; i++)
            {
                TournamentStatkeeper tournamentStatkeeperTemp = new TournamentStatkeeper();

                tournamentStatkeeperTemp.CreatedDateTime = DateTime.UtcNow;
                tournamentStatkeeperTemp.LastModifiedDateTime = DateTime.UtcNow;
                tournamentStatkeeperTemp.CreatedBy = email;
                tournamentStatkeeperTemp.LastModifiedBy = email;
                tournamentStatkeeperTemp.TournamentId = tournamentId;
                tournamentStatkeeperTemp.StatkeeperId = createTournament.StatkeeperIdList[i];
                tournamentStatkeeperList.Add(tournamentStatkeeperTemp);
            }
            return tournamentStatkeeperList;
        }

        public GetTournament MapTournamentById(Tournament tournament)
        {
            TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

            var config = TypeAdapterConfig.GlobalSettings;
            TypeAdapterConfig<(Tournament tournament, string branchName, string activityName), GetTournament>.NewConfig()
            //.Map(dest => dest.BranchName, src => src.branchName)
            .Map(dest => dest.ActivityName, src => src.activityName)
            .Map(dest => dest, src => src.tournament);

            GetTournament getTournament = (tournament, tournament.Name, tournament.Activity.Name).Adapt<GetTournament>();

            return getTournament;
        }

        public TournamentTeam MapTournamentTeamForCreate(long tournamentId, long teamId, string email)
        {
            DateTime dateTime = DateTime.UtcNow;
            TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

            var config = TypeAdapterConfig.GlobalSettings;
            TypeAdapterConfig<(long tournamentId, long teamId, string email, DateTime dateTime), TournamentTeam>.NewConfig()
            .Map(dest => dest.CreatedBy, src => src.email)
            .Map(dest => dest.LastModifiedBy, src => src.email)
            .Map(dest => dest.CreatedDateTime, src => src.dateTime)
            .Map(dest => dest.LastModifiedDateTime, src => src.dateTime)
            .Map(dest => dest.TeamId, src => src.teamId)
            .Map(dest => dest.TournamentId, src => src.tournamentId)
            .Map(dest => dest.IsAccepted, src => false);

            TournamentTeam tournamentTeam = (tournamentId, teamId, email, dateTime).Adapt<TournamentTeam>();

            return tournamentTeam;
        }

        public GetTournamentEvent MapTournamentEventListWithTournament(Tournament tournament, List<Application.Response.Tournament.Event> eventDetailList, bool checkIfTheSeasonIsInProgressOrFinished)
        {
            string tournamentFormat = checkIfTheSeasonIsInProgressOrFinished != true ? Domain.Enums.Enum.TournamentFormat.Season.ToString() : Domain.Enums.Enum.TournamentFormat.PlayOff.ToString();
            TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

            var config = TypeAdapterConfig.GlobalSettings;
            TypeAdapterConfig<(Tournament tournament, List<Application.Response.Tournament.Event> eventDetailList, string tournamentFormat), GetTournamentEvent>.NewConfig()
            .Map(dest => dest.TournamentName, src => tournament.Name)
            .Map(dest => dest.TournamentFormat, src => tournamentFormat)
            .Map(dest => dest.EventDetailList, src => eventDetailList);

            GetTournamentEvent getTournamentEvent = (tournament, eventDetailList, tournamentFormat).Adapt<GetTournamentEvent>();

            return getTournamentEvent;
        }

        public Tournament UpdatePrizeMapper(Tournament tournament, UpdatePrizeReq updatePrizeReq)
        {
            tournament.FirstPrize = updatePrizeReq.FirstPrize;
            tournament.SecondPrize = updatePrizeReq.SecondPrize;
            tournament.ThirdPrize = updatePrizeReq.ThirdPrize;

            return tournament;
        }
        public Tournament UpdateTournament(Tournament tournament, UpdateTournamentReq updateTournament,long activityId)
        {
            tournament.StartDateTime = updateTournament.StartDateTime != null ? updateTournament.StartDateTime : tournament.StartDateTime;
            tournament.EndDateTime = updateTournament.EndDateTime != null ? updateTournament.EndDateTime : tournament.EndDateTime;
            tournament.MinPlayerPerTeam = updateTournament.MinPlayerPerTeam != null ? updateTournament.MinPlayerPerTeam : tournament.MinPlayerPerTeam;
            tournament.MaxPlayerPerTeam = updateTournament.MaxPlayerPerTeam != null ? updateTournament.MaxPlayerPerTeam : tournament.MaxPlayerPerTeam;
            tournament.TeamCapacity = updateTournament.TeamCapacity != null ? updateTournament.TeamCapacity:tournament.TeamCapacity ;
            tournament.ActivityId = activityId;
            tournament.Name = updateTournament.Name != null ? updateTournament.Name : tournament.Name;
            tournament.SeasonStartDate = updateTournament.SeasonStartDate!= null ? updateTournament.SeasonStartDate: tournament.SeasonStartDate;
            tournament.SeasonEndDate = updateTournament.SeasonEndDate!= null ? updateTournament.SeasonEndDate: tournament.SeasonEndDate;
            tournament.SeasonTeamCapacity = updateTournament.SeasonTeamCapacity != null ? updateTournament.SeasonTeamCapacity : tournament.SeasonTeamCapacity;

            return tournament;
        }

        private List<TournamentPlayoffResponse> MapRoundsWithEvents(List<TournamentPlayoffResponse> tournamentPlayoffResponseList)
        {
            for (int i = 0; i < tournamentPlayoffResponseList.Count(); i++)
            {
                if (tournamentPlayoffResponseList[i].TeamDetailList.Where(x => x.Round == 0).FirstOrDefault() != null && tournamentPlayoffResponseList[i].TeamDetailList.Where(x => x.Round == 1).FirstOrDefault() != null)
                {
                    tournamentPlayoffResponseList[i].Round = 1;
                }
                else if (tournamentPlayoffResponseList[i].TeamDetailList.Where(x => x.Round == 1).FirstOrDefault() != null && tournamentPlayoffResponseList[i].TeamDetailList.Where(x => x.Round == 2).FirstOrDefault() != null)
                {
                    tournamentPlayoffResponseList[i].Round = 2;
                }
                else if (tournamentPlayoffResponseList[i].TeamDetailList.Where(x => x.Round == 2).FirstOrDefault() != null && tournamentPlayoffResponseList[i].TeamDetailList.Where(x => x.Round == 3).FirstOrDefault() != null)
                {
                    tournamentPlayoffResponseList[i].Round = 3;
                }
                else if (tournamentPlayoffResponseList[i].TeamDetailList.Where(x => x.Round == 3).FirstOrDefault() != null && tournamentPlayoffResponseList[i].TeamDetailList.Where(x => x.Round == 4).FirstOrDefault() != null)
                {
                    tournamentPlayoffResponseList[i].Round = 4;
                }
                else
                {
                    tournamentPlayoffResponseList[i].Round = 5;
                }
            }
            return tournamentPlayoffResponseList;
        }

        private List<PlayoffGroups> MapEventsIntoGroups(List<TournamentPlayoffResponse> tournamentPlayoffResponseList, List<double> NumberOfGamesPerRound)
        {
            List<PlayoffGroups> playoffGroupList = new List<PlayoffGroups>();
            int skip = 0;
            for (int i = 0; i < NumberOfGamesPerRound.Count; i++)
            {
                PlayoffGroups playoffGroups = new PlayoffGroups();

                playoffGroups.RoundName = i == 0 ? TournamentRound.Qualifiers.ToString() : i == 1 ? TournamentRound.Quarter.ToString() : i == 2 ? TournamentRound.Semi.ToString() : i == 3 ? TournamentRound.Final.ToString() : string.Empty;
                playoffGroups.RoundName = string.Concat(playoffGroups.RoundName, " Round");
                int take = Convert.ToInt32(NumberOfGamesPerRound[i]);

                var roundMatchList = tournamentPlayoffResponseList.Skip(skip).Take(take).ToList();
                playoffGroups.tournamentPlayoffResponseList = roundMatchList;
                skip += take;
                playoffGroupList.Add(playoffGroups);
            }

            return playoffGroupList;
        }

        /// <summary>
        /// divide number of team by 2 until it return 1 (recursive call)
        /// </summary>
        /// <param name="numberOfTeams">count of teams in the tournament</param>
        /// <param name="round">number of times we need to divide it by 2 until we get 1</param>
        /// <returns>number of teams divide by two result</returns>
        private int CalculateNumberOfRound(int numberOfTeams, ref int round)
        {
            int divideBy = 2;
            round++;
            return numberOfTeams / divideBy;
        }

        private List<GetTournamentRoundPlayoffs> MapTournamentEventWithRound(List<PlayoffGroups> playoffGroupList, int round)
        {
            List<GetTournamentRoundPlayoffs> getTournamentRoundPlayoffList = new List<GetTournamentRoundPlayoffs>();
            int skip = 0;
            int take = 4;

            foreach (TournamentRound tournamentRound in Enum.GetValues(typeof(TournamentRound)))
            {
                for (int i = playoffGroupList.Count; i > 0; --i)
                {
                    GetTournamentRoundPlayoffs getTournamentRoundPlayoffs = new GetTournamentRoundPlayoffs();

                    getTournamentRoundPlayoffs.PlayoffGroupList = new List<PlayoffGroups>();
                    getTournamentRoundPlayoffs.Round = tournamentRound.ToString();
                }
            }

            return getTournamentRoundPlayoffList;
        }


        // N = Initial Team Count
        // R = Zero-Based Round #
        // Games = (N / (2 ^ R)) / 2
        private double GamesPerRound(int totalTeams, int currentRound)
        {
            var result = (totalTeams / Math.Pow(2, currentRound)) / 2;

            // Happens if you exceed the maximum possible rounds given number of teams
            if (result < 1.0F) throw new InvalidOperationException();

            return result;
        }

        /// <summary>
        /// Map all tournament Events into a group based response
        /// this is just a first implementation can be improved
        /// </summary>
        /// <param name="tournamentPlayoffResponseList">contains all the events in the tournament</param>
        /// <returns></returns>
        public List<PlayoffGroups> MapTournamentPlayOffResponse(List<TournamentPlayoffResponse> tournamentPlayoffResponseList, int numberOfTeams)
        {
            List<PlayoffGroups> playoffGroupList = new List<PlayoffGroups>();
            List<GetTournamentRoundPlayoffs> getTournamentRoundPlayoffList = new List<GetTournamentRoundPlayoffs>();
            int round = 0;
            List<double> NumberOfGamesPerRound = new List<double>();
            int originalNumberOfTeam = numberOfTeams;
            do
            {
                numberOfTeams = CalculateNumberOfRound(numberOfTeams, ref round);
            } while (numberOfTeams != 1);

            for (int i = 0; i < round; i++)
            {
                double res = GamesPerRound(originalNumberOfTeam, i);
                NumberOfGamesPerRound.Add(res);
            }

            tournamentPlayoffResponseList = MapRoundsWithEvents(tournamentPlayoffResponseList);
            playoffGroupList = MapEventsIntoGroups(tournamentPlayoffResponseList, NumberOfGamesPerRound);

            return playoffGroupList;
        }

        public List<int> CalculateSeeding(int numTeams)
        {
            //This line calculates the number of rounds required for the tournament. It uses logarithms to determine the number of rounds based on the number of teams.
            double rounds = Math.Log(numTeams) / Math.Log(2) - 1;

            //This list represents the current seeding order for the first round
            List<int> pls = new List<int> { 1, 2 };

            for (int i = 0; i < rounds; i++)
            {
                pls = NextLayer(pls);
            }

            //now it contains the seeding order for the last round
            return pls;
        }

        //function generates the seeding order for the next round based on the current round's seeding order.
        private List<int> NextLayer(List<int> pls)
        {
            List<int> outList = new List<int>();

            //This line calculates the length of the next round's seeding order
            int length = pls.Count * 2 + 1;

            foreach (int d in pls)
            {
                //Adds the current element
                outList.Add(d);
                //Adds the complement of d with respect to length to outList
                outList.Add(length - d);
            }

            return outList;
        }

        public List<MatchUpDetails> CreateMatchUpsForPlayOff(List<int> matchesPattern, List<long> seasonTopTeams)
        {
            List<MatchUpDetails> matchDetailList = new List<MatchUpDetails>();

            for (int i = 0; i < matchesPattern.Count; i++)
            {
                MatchUpDetails matchUpDetails = new MatchUpDetails();

                var firstOpponent = seasonTopTeams[matchesPattern[i] - 1];
                var secondOpponent = seasonTopTeams[matchesPattern[i + 1] - 1];

                matchUpDetails.TeamList = new List<long>();

                matchUpDetails.TeamList.Add(firstOpponent);
                matchUpDetails.TeamList.Add(secondOpponent);

                matchDetailList.Add(matchUpDetails);
                i++;
            }

            return matchDetailList;
        }
        public List<MatchUpDetails> AssignRandomStatkeeperAndVenueToMatchUps(DateTime startDate, DateTime endDate, List<MatchUpDetails> matchUps, List<AllStatkeepersVenueList> allStatkeepersVenueList)
        {
            // Shuffle the list randomly
            Random rand = new Random();
            matchUps = matchUps.OrderBy(x => rand.Next()).ToList();

            for (int i = 0; i < matchUps.Count; i++)
            {
                // Generate a random date within the given range
                TimeSpan timeSpan = endDate - startDate;
                int daysToAdd = rand.Next(timeSpan.Days);
                DateTime matchDate = startDate.AddDays(daysToAdd);
                matchUps[i].EventDateTime = matchDate;

                AllStatkeepersVenueList statkeeperWithContainVenueList = allStatkeepersVenueList[rand.Next(allStatkeepersVenueList.Count)];

                if (statkeeperWithContainVenueList is not null)
                {
                    long venueId = statkeeperWithContainVenueList.VenueList[rand.Next(statkeeperWithContainVenueList.VenueList.Count)].Id;

                    matchUps[i].VenueId = venueId;
                    matchUps[i].StatkeeperId = statkeeperWithContainVenueList.UserId;
                }
            }

            return matchUps;
        }
        public Notifications MapTournamentWithNotification(string email, long userId)
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
    }
}
