namespace University_Management_System.Models.MainModels
{
    public class AdminProfile
    {
        public int Id { get; set; }

        public string? AdminId { get; set; }

        public string? UserId { get; set; }

        public string AdminName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Status { get; set; } = "Inactive";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

