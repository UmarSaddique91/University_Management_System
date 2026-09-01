using Microsoft.AspNetCore.Identity;
namespace University_Management_System.Data
{
    public class ApplicationUser: IdentityUser
    {
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }
}
