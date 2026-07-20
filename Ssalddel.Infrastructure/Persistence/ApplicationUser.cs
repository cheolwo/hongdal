using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace 살뜰.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? BusinessRegistrationNumber { get; set; }

        [MaxLength(64)]
        public string? PrivacyConsentVersion { get; set; }

        public DateTime? PrivacyConsentedAtUtc { get; set; }
    }
}
