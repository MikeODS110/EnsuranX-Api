namespace Ensuranx.Common.GetNotificationMessages;

public class GetNotificationMessage
{
    GetNotificationMessage() { }

    static public GetNotificationMsg GetFollowRequestNotification(string followerName)
    {
        GetNotificationMsg getNotificationMsg = new GetNotificationMsg();

        getNotificationMsg.Title = $"{followerName} wants to follow you.";
        getNotificationMsg.Body = "";

        return getNotificationMsg;
    }

    static public GetNotificationMsg GetAcceptedFollowRequestNotification(string followerName)
    {
        GetNotificationMsg getNotificationMsg = new GetNotificationMsg();

        getNotificationMsg.Title = $"{followerName} started following you.";
        getNotificationMsg.Body = "";

        return getNotificationMsg;
    }
    static public GetNotificationMsg GetTeamMembetoJoinTeamNotification(string UserName, string teamName)
    {
        GetNotificationMsg getNotificationMsg = new GetNotificationMsg();

        getNotificationMsg.Title = $"{UserName} invites you to join the team  {teamName}";
        getNotificationMsg.Body = "";

        return getNotificationMsg;
    }
    static public GetNotificationMsg PlayerAcceptedCaptainRequest(string teamName)
    {
        GetNotificationMsg getNotificationMsg = new GetNotificationMsg();

        getNotificationMsg.Title = $"You've joined the team {teamName} .";
        getNotificationMsg.Body = "";

        return getNotificationMsg;
    }
    static public GetNotificationMsg JoinToTournamentRequest(string tournamentName, string teamName)
    {
        GetNotificationMsg getNotificationMsg = new GetNotificationMsg();

        getNotificationMsg.Title = $"{teamName} wants to join the Tournament  {tournamentName}";
        getNotificationMsg.Body = "";

        return getNotificationMsg;
    }
}
