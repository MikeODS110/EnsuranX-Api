using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensuranx.Domain.Common.Errors
{
    public static partial class Errors
    {
        public static class Event
        {
            public static Error TeamNotFound => Error.NotFound(code: "User.TeamNotFound", description: "Provided teams are not present in the collection");
            public static Error EventNotFound => Error.NotFound(code: "User.EventNotFound", description: "Event not found");
            public static Error ErrorSameTeam => Error.NotFound(code: "User.ErrorSameTeam", description: "Can not schedule due to duplicate opponents");
            public static Error LobbyNotFound => Error.NotFound(code: "User.LobbyNotFound", description: "Lobby not found");
            public static Error EventHaveNoStats => Error.NotFound(code: "User.EventHaveNoStats", description: "Event have no stats");
            public static Error DuplicateTeamMember => Error.Validation(code: "User.DuplicateTeamMember", description: "Team members in each team should be equal and not repeating");
            public static Error NoEventCollectionFound => Error.Validation(code: "User.NoEventCollection", description: "No EventCollection found");
        }
    }
}
