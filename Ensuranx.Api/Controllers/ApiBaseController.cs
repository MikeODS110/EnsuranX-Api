using ErrorOr;
using Ensuranx.Api.Common.ClaimsTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ensuranx.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiBaseController : Controller
    {
        protected long UserID { get; private set; } = 0;
        protected int RoleID { get; private set; } = 0;
        protected string Email { get; private set; } = string.Empty;
        protected IActionResult Problem(List<Error> errors)
        {
            HttpContext.Items["errors"] = errors;

            var firstError = errors[0];

            var statusCode = firstError.Type switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError,
            };
            return Problem(statusCode: statusCode, title: firstError.Description);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
             
            string? userId = this.User.Claims.FirstOrDefault(x => x.Type == UserClaimTypes.UserID)?.Value;
            string? email = this.User.Claims.FirstOrDefault(x => x.Type == UserClaimTypes.Email)?.Value;
            string? roleId = this.User.Claims.FirstOrDefault(x => x.Type == UserClaimTypes.RoleID)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                UserID = Convert.ToInt64(userId);
            }
            if (!string.IsNullOrEmpty(email))
            {
                Email = email;
            }
            if (!string.IsNullOrEmpty(roleId))
            {
                RoleID = Convert.ToInt32(roleId);
            }
        }
    }
}


