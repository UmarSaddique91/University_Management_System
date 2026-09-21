using System.ComponentModel.DataAnnotations;

namespace University_Management_System.DTOs
{
    public class AdminProfileDTO
    {
        [Required]
        public string AdminName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

