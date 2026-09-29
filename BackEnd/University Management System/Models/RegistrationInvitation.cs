namespace University_Management_System.Models
{
    public class RegistrationInvitation
    {
        public int Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public int ProfileId { get; set; }

        public string? RecipientEmail { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

