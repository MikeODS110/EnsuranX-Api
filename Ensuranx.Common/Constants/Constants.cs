namespace Ensuranx.Common.Constants
{
    public static class Constants
    {
        public class APIErrorMessages
        {
            public const string USER_DEFAULT_PROFILE_IMAGE = "https://firebasestorage.googleapis.com/v0/b/Ensuranx-aed4d.appspot.com/o/ProfileImages%2FUser%20Default.png?alt=media&token=a16333c4-e69d-4b91-a7ca-6659731a1805";
            public const string USER_DEFAULT_COVER_IMAGE = "https://firebasestorage.googleapis.com/v0/b/Ensuranx-aed4d.appspot.com/o/ProfileImages%2FCover%20(2).png?alt=media&token=ca0118d8-6b10-4041-a320-cfc9f6200ba0";
            public const string BRANCH_DEFAULT_PROFILE_IMAGE = "https://firebasestorage.googleapis.com/v0/b/Ensuranx-aed4d.appspot.com/o/ProfileImages%2FBranch%20Default.png?alt=media&token=3dd8b069-9825-4dd2-bd24-289ce15bcce2";
            public const string BRANCH_DEFAULT_COVER_IMAGE = "https://firebasestorage.googleapis.com/v0/b/Ensuranx-aed4d.appspot.com/o/ProfileImages%2FCover%20(2).png?alt=media&token=ca0118d8-6b10-4041-a320-cfc9f6200ba0";

            //Apis Success And Error Messages

            public const string TRY_CATCH_ERROR = "Oops! Something went wrong. Please contact Ensuranx customer support";
            public const string DUPLICATE_PHONE_NUMBER = "Phone number is already registered";
            public const string PASSWORD_DID_NOT_MATCHED = "Passwords did not matched";
            public const string DUPLICATE_EMAIL_ADDRESS = "Email is already registered";
            public const string DUPLICATE_USERNAME = "Username is already taken";
            public const string USER_NOT_CREATED = "User not created sucessfully";
            public const string USER_NOT_FOUND = "User not Found";
            public const string USER_CREATED = "User created sucessfully";
            public const string USER_DELETED = "User deleted sucessfully";
            public const string BRANCH_DELETED = "Branch deleted sucessfully";
            public const string EVENTCOLLECTION_DELETED = "EventCollection deleted sucessfully";
            public const string BRANCH_CREATED = "Branch created sucessfully";
            public const string EVENT_CREATED = "Event created sucessfully";
            public const string EVENT_INPROGRESS = "Event inprogress already";
            public const string EVENT_STARTED_SUCCESSFULLY = "Event started successfully";
            public const string EVENT_UPDATED = "Event updated sucessfully";
            public const string PERMISSSIONS_UPDATED = "Permisssions updated sucessfully";
            public const string TEAM_CREATED = "Team created sucessfully";
            public const string BRANCH_UPDATED = "Branch updated sucessfully";
            public const string INTERESTS_CREATED = "Interests sucessfully Added";
            public const string LOBBY_CREATED = "Lobby sucessfully created";
            public const string VERIFIED_EMAIL = "Email is sucessfully verified";
            public const string RESEND_EMAIL = "Resend Email sucessfully";
            public const string EMAIL_NOT_VERIFIED = "Please Verify your email before login";
            public const string INCORRECT_PASSWORD = "Incorrect Password";
            public const string AUTHORIZATION_HEADER_NOT_FOUND = "No Authorization headers are present!";
            public const string INVALID_USER_REQUEST = "Invalid user request";
            public const string RECORD_UPDATED = "Record Updated Successfully";
            public const string REQUESTSENT = "Request sent sucessfully";
            public const string REQUESTACCEPTED = "Request Accepted sucessfully";
            public const string REQUESTREJECTED = "Request Rejected sucessfully";
            public const string UNFOLLOW = "UnFollow sucessfully";
            public const string SUBSTITUTEDSUCCESFULLY = "Substitute sucessfully";
            public const string CANNOTSUBSTITUTE = "Player can not be substitute";
            public const string DISABLE = "Disable successfully";
            public const string ALREADYEXIST = "Player already exist";
            public const string LOBBY_ALREADY_EXIST = "Active lobby already exist";
            public const string DELETESUCCESSFULLY= "Record Deleted Succesfully";
            public const string PLAYERBACKTOTHELOBBY = "Player back to the lobby";
            public const string PLAYERDELETED = "Player deleted successfully";
            public const string PLAYER_ALREADY_A_MEMBER = "Player is already a member";



            //Required Constants

            public const string ROLE_REQUIRED = "Role Name is required";
            public const string NAME_REQUIRED = "Name is required";
            public const string EMAIL_REQUIRED = "Email is required";
            public const string ZIPCODE_REQUIRED = "ZipCode is required";
            public const string USERNAME_REQUIRED = "Username is required";
            public const string PHONENUMBER_REQUIRED = "Phone Number is required";
            public const string ADDRESS_REQUIRED = "Address is required";
            public const string BUSINESS_REQUIRED = "Business is required";
            public const string BRANCH_REQUIRED = "Branch is required";
            public const string VENUE_REQUIRED = "Venue is required";
            public const string PASSWORD_REQUIRED = "Password is required";
            public const string CREDITCARD_REQUIRED = "Card Number is required";
            public const string CVV_REQUIRED = "Cvv is required";
            public const string EXPIRE_DATE_REQUIRED = "Expire date is required";
            public const string PAYMENT_OPTION_REQUIRED = "Payment option is required";
            public const string PASSWORD_CONFIRM_REQUIRED = "Confirm password is required";
            public const string PASSWORD_CONFIRM_DONOT_MATCH = "The password and confirmation password do not match.";
            public const string OTP_REQUIRED = "Otp Code is required";
            public const string OTP_SENT = "Otp Sent Successfully";
            public const string USER_REQUIRED = "User is required";
            public const string ACTIVITIES_REQUIRED = "Activities are required";
            public const string OLD_PASSWORD_REQUIRED = "Old password is required";
            public const string NEW_PASSWORD_REQUIRED = "New password is required";
            public const string CONFIRM_PASSWORD_REQUIRED = "Confirm password is required";
            public const string PASSWORD_DIDNT_MATCHED = "Passwords didn't matched";

            //Email Constants
            public const string EMAIL_FROM = "Ensuranx";
            public const string TEAM = "Team ";
            public const int TOURNAMENT_TEAM_COUNT = 16;
            public const int SHOT_CLOCK_TIMER = 24;
            public const string NO_DATE_FOUND = "No date found";

            //Success Messages
            public const string PASSWORD_UPDATED_SUCESSFULLY = "Password updated successfully";
            public const string NOTIFICATION_ENABLE_SUCESSFULLY = "Notication enable successfully";
            public const string NOTIFICATION_DISEABLE_SUCESSFULLY = "Notication disable successfully";
            public const string CREDIT_CARD_ADDED_SUCESSFULLY = "Credit card added successfully";
            public const string PACKAGE_ADDED_SUCESSFULLY = "Package added successfully";
            public const string VENUE_CREATED_SUCESSFULLY = "Venue created successfully";
            public const string STATKEEPER_CREATED_SUCESSFULLY = "Statkeeper created successfully";
            public const string STATKEEPER_DELETED_SUCESSFULLY = "Statkeeper deleted successfully";
            public const string TOURNAMENT_CREATED_SUCESSFULLY = "Tournament created successfully";
            public const string STATKEEPER_TOURNAMENT_SUCESSFULLY = "Statkeeper added to the tournament successfully";
            public const string REQUEST_CREATED_SUCESSFULLY = "Your request send successfully";
            public const string REQUEST_ACCEPTED_SUCESSFULLY = "Request accepted successfully";
            public const string REQUEST_REJECTED_SUCESSFULLY = "Request rejected successfully";
            public const string MATCHES_RANDOMIZE_SUCESSFULLY = "Matches randomize successfully";
            public const string SCORE_UPDATED_SUCESSFULLY = "Score updated successfully";
            public const string TEAM_UPDATED_SUCESSFULLY = "Team updated successfully";
            public const string TEAM_OUT_OF_LIMIT = "Team out of limit";
            public const string LOGOUT_SUCCESS = "Logout successfully";
            public const string LOBBY_STATUS_UPDATED = "Lobby status updated";
        }
    }
}
