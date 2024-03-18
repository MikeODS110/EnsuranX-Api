using ErrorOr;

namespace Ensuranx.Domain.Common.Errors
{
    public static partial class Errors
    {
        public static class User
        {
            public static Error DuplicateEmail => Error.Validation(code: "User.DuplicateEmail", description: "Email is already in use");
            public static Error BranchRequired => Error.Validation(code: "User.BranchRequired", description: "Please provide valid branch id");
            public static Error ErrorImageUpdate => Error.Validation(code: "User.ImageUpdate", description: "Please provide image id to update image");
            public static Error UserNotFoundWithThisEmail => Error.Validation(code: "User.NotFound", description: "User not exist with this email");
            public static Error DuplicateUsername => Error.Validation(code: "User.DuplicateUserName", description: "Username is already in use");
            public static Error DuplicatePhoneNumber => Error.Validation(code: "User.DuplicatePhoneNumber", description: "Phone Number is already in use");
            public static Error UserNotCreatedSuccessfully => Error.Failure(code: "User.NotCreated", description: "User not created sucessfully");
            public static Error UserNotDeletedSuccessfully => Error.Failure(code: "User.NotDeleted", description: "User not deleted sucessfully");
            public static Error ExceptionMessage => Error.Unexpected(code: "User.Except", description: "Oops! Something went wrong. Please contact Ensuranx customer support");
            public static Error RecordNotFound => Error.Unexpected(code: "User.Record Not Found", description: "Record Not Found");
            public static Error RequestNotFound => Error.Unexpected(code: "User.RequestNotFound", description: "Request Not Found");
            public static Error DuplicateInterest => Error.Validation(code: "User.DuplicateInterest", description: "Duplicate Interest Found");
            public static Error UserHaveNoInterest => Error.NotFound(code: "User.UserHaveNoInterest", description: "User Have No Interests");
            public static Error UserHaveNoBranches => Error.NotFound(code: "User.UserHaveNoBranches", description: "User Have No Branches");
            public static Error NoPackagesFound => Error.NotFound(code: "User.NoPackagesFound", description: "No packages found");
            public static Error NoBusinessTypeFound => Error.NotFound(code: "User.NoBusinessTypeFound", description: "No business type found");
            public static Error NoPaymentOptionFound => Error.NotFound(code: "User.NoPaymentOptionFound", description: "No payment option found in system. Please contact Ensuranx customer support");
            public static Error NoStatkeeperFound => Error.NotFound(code: "User.NoStatkeeperFound", description: "No Statkeeper Found");
            public static Error ErrorUserProfileUpdate => Error.Failure(code: "User.NotUpdated", description: "Some error occur while updating user profile");
            public static Error ErrorUserPasswordUpdate => Error.Failure(code: "User.PassNotUpdated", description: "Password not updated sucessfully");
            public static Error ErrorOldPasswordMisMatched => Error.Validation(code: "User.PassNotMatch", description: "Old Password mis-matched");
            public static Error ErrorInvalidOtpCode => Error.Validation(code: "User.InvalidOtpCode", description: "Invalid Otp Code");
            public static Error ErrorUserNotFound => Error.Validation(code: "User.UserNotFound", description: "User not found");
            public static Error ErrorNoTeamFound => Error.NotFound(code: "ErrorNoTeamFound", description: "No Team Found");
            public static Error NoDataFound => Error.Unexpected(code: "User.Except", description: "No data found for respective requriment");
            public static Error OTPSent => Error.Unexpected(code: "User.OTPSent", description: "OTP Sent successfully");

        }
    }
}
