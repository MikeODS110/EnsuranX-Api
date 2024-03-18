using ErrorOr;

namespace Ensuranx.Domain.Common.Errors;

public static partial class Errors
{
    public static class Notification
    {
        public static Error NoNotificationFound => Error.NotFound(
           code: "Auth.NoNotificationFound",
           description: "No Notification Found");
    }
}
