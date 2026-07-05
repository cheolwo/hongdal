using Microsoft.AspNetCore.Identity;

namespace 홍달.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? BusinessRegistrationNumber { get; set; }
    }
}
