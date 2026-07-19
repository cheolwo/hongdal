using Microsoft.AspNetCore.Identity;

namespace 살뜰.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? BusinessRegistrationNumber { get; set; }
    }
}
