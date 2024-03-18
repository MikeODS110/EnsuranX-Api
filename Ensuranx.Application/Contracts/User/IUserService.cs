
using ErrorOr;
using Ensuranx.Application.Requests.Identity;
using Ensuranx.Application.Response.User;
using Ensuranx.Common.WrapperInterface;
using Ensuranx.Domain.IdentityExtensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ensuranx.Application.Contracts.Role.User
{
    public interface IUserService
    {
        Task<IResult<List<AppUser>>> GetAllAsync();
        Task<ErrorOr<ValidateUserDetailResponse>> AuthenticateUserDetailsAsync(ValidateUserDetails validateUserDetails);
        Task<ErrorOr<AppUser>> GetUserByEmail(string email);
        public int createGuId();
        Task<ErrorOr<RegistrationResponse>> RegisterAsync(RegistrationRequest request);
        public Task<ErrorOr<LoginResponse>> LoginAsync(LoginRequest userModel);
        public SigningCredentials GetSigningCredentials();
        public JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims);
        public Task<List<Claim>> GetClaims(AppUser user);



    }
}
