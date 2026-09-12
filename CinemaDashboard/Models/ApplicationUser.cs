using Microsoft.AspNetCore.Identity;

namespace CinemaDashboard.Models
{
    // Extend IdentityUser here if you need extra profile fields later
    // (e.g. FullName, ProfileImage, etc.)
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
