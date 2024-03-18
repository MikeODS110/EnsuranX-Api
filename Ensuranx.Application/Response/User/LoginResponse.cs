namespace Ensuranx.Application.Response.User
{
    public class LoginResponse
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Role { get; set; } = string.Empty;
        
        public bool IsEmailVerified { get; set; } = false;
        public bool IsAddedBusinessDetail { get; set; } = false;
    }
}
