using ErrorOr;

namespace Ensuranx.Domain.Common.Errors
{
    public static partial class Errors
    {
        public static class Authentication
        {
            public static Error InvalidCredientials => Error.Validation(
            code: "Auth.InvalidCred",
            description: "Invalid credientials");
            
            public static Error UserNotFound => Error.Validation(
            code: "Auth.NotFound",
            description: "User Not Found");

            public static Error EmailTemplateNotFound => Error.Validation(
            code: "Auth.NotFound",
            description: "Email Template Not Found");

            public static Error VerifyYourEmail => Error.Validation(
            code: "Auth.NotVerified",
            description: "Verify your email before login");

            public static Error ExceptionMessage => Error.Unexpected(code: "User.Except", description: "Oops! Something went wrong. Please contact Ensuranx customer support");
        }
    }
}
