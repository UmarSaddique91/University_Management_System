using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace University_Management_System.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string? TeacherId { get; set; }
        public string?UserId { get; set; }
        public string? Email { get; set; }
        public string? TeacherName { get; set; }

        

        public string? Specialization { get; set; }

        public string? Contact { get; set; }

        public string? Status { get; set; } = "Active";

        
        public int? DepartmentId { get; set; }

        [JsonIgnore]
        public Department? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

