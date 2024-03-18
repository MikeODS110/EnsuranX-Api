using ErrorOr;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Requests.Profile;
using Ensuranx.Application.Response.Other;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.PaginationResponse;
using Ensuranx.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Ensuranx.Application.Contracts.UserInfo
{
    public interface IUserInfoService
    {
        public Task<Ensuranx.Domain.Entities.UserInfo> CreateUserInfo(Ensuranx.Domain.Entities.UserInfo userInfo);
        public Task<Ensuranx.Domain.Entities.UserInfo> UpdateUserInfo(Ensuranx.Domain.Entities.UserInfo userInfo);
        public Task<Ensuranx.Domain.Entities.UserInfo> GetUserInfoById(long userInfoId);
        public Ensuranx.Domain.Entities.UserInfo GetUserInfoByUserId(long userInfoId);
        //public Task<GenericObj> GetVenueStatkeepr(long venueId, long statkeeperId);
        public Ensuranx.Domain.Entities.UserInfo GetUserInfoByUserIdAsync(int userId);
        public Task<ErrorOr<VerifyOtpResponse>> VerifyOtpService(int otpCode, string email);
        public Task<ErrorOr<GenericMessage>> SaveProfileDetails(AddProfileDetails addProfileDetails, long userId, IConfiguration _firebasePath);
        public Task<ErrorOr<PermissionResponse>> GetPermissionsAsync(long userId, string email);
        public Task<ErrorOr<ForgetPasswordResponse>> ForgetPasswordOtpAsync(ForgetPasswordOtpRequest forgetPasswordRequest);
        public Task<ErrorOr<ForgetPasswordResponse>> ForgetPasswordAsync(ForgetPasswordRequest forgetPasswordRequest);
        public Task<ErrorOr<PasswordUpdatedResponse>> UpdatePasswordAsync(UpdatePassword updatePassword, long userId);
        public Task<ErrorOr<GenericMessage>> UpdatePermissionsAsync(long userId, string email, UpdatePermission updatePermission);

        public Task<ErrorOr<GenericMessage>> DeleteUserAccountAsync(long userId, string email, int roleId);
        public Task<ErrorOr<VerifyUserPasswordResponse>> VerifyUserPasswordAsync(long userId, string email, VerifyUserPassword verifyUserPassword);

    }
}
