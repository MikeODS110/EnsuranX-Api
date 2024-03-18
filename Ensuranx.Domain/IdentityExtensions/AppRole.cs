using Microsoft.AspNetCore.Identity;

namespace Ensuranx.Domain.IdentityExtensions
{
    public class AppRole : IdentityRole<int>
    {
        public string Description { get; set; } = string.Empty;
    }
}
