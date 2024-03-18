using ErrorOr;

namespace Ensuranx.Domain.Common.Errors
{
    public static partial class Errors
    {
        public static class Team
        {
            public static Error TeamIsRequired => Error.Validation(code: "User.TeamIsRequired", description: "Please select team to add member");
            public static Error TeamLimitExceeded => Error.Validation(code: "User.TeamLimitExceeded", description: "Team Limit Exceeded");
            public static Error TeamNameIsTaken => Error.Validation(code: "User.TeamNameIsTaken", description: "Team is already registered with this name");
            public static Error ValidTeamTypeIsRequired => Error.Validation(code: "User.ValidTeamTypeIsRequired", description: "Please select valid team type");
            public static Error NoTeamsFound => Error.NotFound(code: "User.NoTeamsFound", description: "No Teams Found");
            public static Error NoVenueActivityFound => Error.NotFound(code: "User.NoVenueActivityFound", description: "No Venue Activity Found");
            public static Error NoTeamActivityFound => Error.NotFound(code: "User.NoTeamActivityFound", description: "No Team Activity Found");
            public static Error NoLobbyFound => Error.NotFound(code: "User.NoLobbyFound", description: "No Lobby Found");
            public static Error PartOfActiveEvent => Error.Validation(code: "User.PartOfActiveEvent", description: "You are a part of an active event");
            public static Error NoTeamMemberFound => Error.NotFound(code: "User.NoTeamMemberFound", description: "No Team Member Found");
            public static Error TeamMemberFound => Error.NotFound(code: "User.TeamMemberFound", description: "Member Already exists");
            public static Error UnAuthorizedToCreateTeam => Error.NotFound(code: "User.UnAuthorizedToCreateTeam", description: "You are not authorize to create team");

            public static Error PermissionDenied => Error.NotFound(code: "User.PermissionDenied", description: "PermissionDenied");
        }
    }
}
