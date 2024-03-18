using Ensuranx.Domain.IdentityExtensions;
using Microsoft.AspNetCore.Identity;
using static Ensuranx.Common.Enums.Enums;

namespace Ensuranx.Application.Response.User
{
    public class Registration
    {
        public ResponseStatus ResponseStatus { get; set; }
        public IEnumerable<IdentityError> Error { get; set; } = Enumerable.Empty<IdentityError>();
        public string Message { get; set; } = string.Empty;
        public UserResponse User { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool EmailConfirmed { get; set; } = false;
    }

    public class RegistrationResponse
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } = default!;
        public string Token { get; set; }
    }
}
