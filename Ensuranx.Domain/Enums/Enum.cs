using System.Diagnostics.Contracts;

namespace Ensuranx.Domain.Enums
{
    public class Enum
    {
        public enum WeekDays
        {
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6,
            Sunday = 7
        }
       
        public enum Roles
        {
            Admin = 1,
            User = 2,
            Guest = 3,
            
        }

        public enum SortDirection
        {
            Ascending,
            Descending
        }

        public enum NotificationsTypes
        {
            Emails = 0,
            FollowRequest = 1,
            TeamJoinRequest = 2,
        }
        
        public enum ImageType
        {
            Profile = 1,
            Cover = 2,
            Media = 3
        }

        public enum ConnectionEntity
        {
            Branch = 1,
            User = 2,
            Activity = 3,
        }

        public enum ConnectionType
        {
            Twitter = 1,
            PhoneNumber = 2,
            Email = 3,
            Facebook = 4,
            Website = 5,
            Address = 7
        }

        public enum NotificationPlatform
        {
            Mobile = 1,
            Email = 2,
            Text = 3,
            Tablet = 4
        }
        
        public enum BusinessPackageType
        {
            Yearly = 1,
            Monthly = 2
        }

        public enum TemplatesEnum
        {
            SignupOtp = 1,
            ResendOtp = 2,
            ForgetPassword = 3,
            StatkeeperSignUp = 4
        }
        
        public enum Platform
        {
            Web = 1,
            Ios = 2,
            Android = 3,
            Tablet = 4,
            Tv = 5,
        }

        public enum PaymentOption
        {
            Stripe = 1,
            ApplePay = 2,
            PayPal = 3
        }
        public enum Permission
        {
            ViewMyProfile = 1,
            ViewMyLastSeen = 2,
            ViewMyPlayingStatus = 3,
            ViewMyLastPlayed = 4
        }

        public enum PermissionType
        {
            Everyone = 1,
            Business = 2,
            Statkeeper = 3,
            Followers = 4,
            None = 5,
        }

        public enum TeamType
        {
            All = 0,
            Temporary = 1,
            Official = 2
        }

        public enum EventType
        {
            Pickup = 1,
            Regulation = 2
        }

        public enum TeamMemberRole
        {
            Captain = 1,
            Member = 2
        }

        public enum EventStatus
        {
            All = 0,
            Active = 1,
            Win = 2,
            Lose = 3,
            Tie = 4,
            InLobby = 5,
            Completed=6,
            InActive=7
        }

        public enum InviteAction
        {
            Accept = 1,
            Reject = 2,
            Cancel = 3
        }
        
        public enum TournamentStatus
        {
            All = 0,
            Upcoming = 1,
            Active = 2,
            Completed = 3,
        }
        
        public enum JoinedStatus
        {
            Joined = 0,
            RequestedPending = 1,
            RequestToJoin = 2
        }

        public enum TournamentJoinStatus
        {
            Joined = 0,
            RequestToJoin = 1,
            RequestedToJoin = 2,
        }

        public enum TournamentType
        {
            SingleElimination = 1,
        }

        public enum StatType
        {
            Counter = 1,
            HitMiss = 2,
            Command = 3,
        }

        public enum BasketballStat
        {
            OnePoint = 1,
            TwoPoint = 2,
            ThreePoint = 3,
            AST = 4,
            REB = 5,
            STLS = 6,
            BLKS = 7
        }
        public enum Follows
        {
            RequestToFollow = 1,
            Following = 2,
            RequestPending = 3,

        }
        public enum Position
        {
            First =1,
            Second=2,
            Third=3,
            Fouth=4,
            Fifth= 5

        }
        public enum TournamentRound
        {
            Qualifiers = 4,
            Quarter = 3,
            Semi = 2,
            Final = 1,
            Season = -1
        }

        public enum TournamentFormat
        {
            Season = 1,
            PlayOff = 2,
        }

        public enum Round
        {
            Qualifiers = 4,
            Quarter = 3,
            Semi = 2,
            Final = 1,
        }

        public enum EventTypeLimit
        {
            Pickup = 5,
            Regulation = 100
        }
    }
}
