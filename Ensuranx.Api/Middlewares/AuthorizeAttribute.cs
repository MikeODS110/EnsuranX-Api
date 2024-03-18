using ErrorOr;
using Ensuranx.Api.Common.ClaimsTypes;
using Ensuranx.Application.Interfaces;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ensuranx.Api.Middlewares
{
    public class AuthorizeAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            ErrorOr<UserInfo> authResult = new ErrorOr<UserInfo>();
            ErrorOr<int> authRoleId = new ErrorOr<int>();
            UserClaimTypes userClaimTypes = new UserClaimTypes();
            string userId = string.Empty;
            string email = string.Empty;
            string role = string.Empty;

            try
            {
                bool authorized = context.ActionDescriptor.EndpointMetadata
                    .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

                if (!authorized)
                {
                    var authHeader = context.HttpContext.Request.Headers["Authorization"];
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        var authService = context.HttpContext.RequestServices.GetService<IUserInfoRepository>();
                        var authRoleService = context.HttpContext.RequestServices.GetService<IRoleRepository>();

                        var jwtHeaderToken = context.HttpContext.Request.Headers["Authorization"][0].Replace("Bearer", string.Empty).Trim();
                        var jwtHandler = new JwtSecurityTokenHandler();
                        var jwtToken = jwtHandler.ReadJwtToken(jwtHeaderToken);

                        userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
                        email = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;
                       

                        authResult = authService.GetUserInfoByUserIdAsync(int.Parse(userId));
                        authRoleId = authRoleService.GetRoleByUserId(int.Parse(userId));

                        var userFound = authResult.Value != null ? true : false;

                        if (!userFound)
                        {
                            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                            return;
                        }
                        else
                        {
                            userId = authResult.Value.Id.ToString();
                            role = authRoleId.Value.ToString();
                        }
                        authorized = !string.IsNullOrEmpty(userId) ? true : false;
                    }


                    if (!authorized)
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(Constants.APIErrorMessages.INVALID_USER_REQUEST);
                        return;
                    }

                    context.HttpContext.User.AddIdentity(new ClaimsIdentity(new List<Claim>() {
                                                         new Claim(UserClaimTypes.UserID, userId),
                                                         new Claim(UserClaimTypes.Email, email),
                                                         new Claim(UserClaimTypes.RoleID,role)
                     }));
                }
                else
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(Constants.APIErrorMessages.AUTHORIZATION_HEADER_NOT_FOUND);
                    return;
                }
            }
            catch (Exception ex)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(Constants.APIErrorMessages.TRY_CATCH_ERROR);
                return;

            }
        }
    }
}
