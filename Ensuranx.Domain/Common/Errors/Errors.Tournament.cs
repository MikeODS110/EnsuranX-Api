using ErrorOr;

namespace Ensuranx.Domain.Common.Errors
{
    public static partial class Errors
    {
        public static class Tournament
        {
            public static Error NoTournamentFound => Error.NotFound(code: "User.NoTournamentFound", description: "No tournament found");
            public static Error NoTournamentTeamFound => Error.NotFound(code: "User.NoTournamentTeamFound", description: "No tournament teams found");
            public static Error NoTournamentResultFound => Error.NotFound(code: "User.NoTournamentResultFound", description: "No tournament result found");
            public static Error NoStatkeeperFound => Error.NotFound(code: "User.NoStatkeeperFound", description: "No statkeeper found");
            public static Error NoEventsFound => Error.NotFound(code: "User.NoEventsFound", description: "No matches found in the tournament");
            public static Error NoTeamRequestFound => Error.NotFound(code: "User.NoTeamRequestFound", description: "No team request found");
            public static Error NoVenueFound => Error.NotFound(code: "User.NoVenueFound", description: "No venue found for tournament");
            public static Error RequestLimitReached => Error.Validation(code: "User.RequestLimitReached", description: "Tournament is locked. Please contact admin to join");
            public static Error TeamMemberCountIsNotValid => Error.Validation(code: "User.TeamMemberCountIsNotValid", description: "Team Member Count is not valid to join this tournament");
            public static Error YouCanNotJoinTournament => Error.Validation(code: "User.YouCanNotJoinTournament", description: "Your team can not join this tournament due to different activity");
            public static Error YourRequestIsPending => Error.Validation(code: "User.YourRequestIsPending", description: "Your request to join the tournament is pending");
            public static Error YourRequestIsAccepted => Error.Validation(code: "User.YourRequestIsAccepted", description: "Your request to join the tournament is already accept");
            public static Error NotEnoughTeamsToRandomize => Error.Validation(code: "User.NotEnoughTeamsToRandomize", description: "Not enough teams to randomize matches");
            public static Error NotEnoughVenuesToRandomize => Error.Validation(code: "User.NotEnoughVenuesToRandomize", description: "Not enough venues to randomize matches");
            public static Error JerseyNoRepeated => Error.NotFound(code: "User.JerseyNoRepeated", description: "Jersey Numbers are Repeated your team. ");
        }
    }
}
