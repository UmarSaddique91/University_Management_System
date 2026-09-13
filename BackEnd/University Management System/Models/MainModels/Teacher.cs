using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace University_Management_System.Models.MainModels
{
    public class Teacher
    {
        public int Id { get; set; }
        public string? TeacherId { get; set; }
        public string? TeacherName { get; set; }

        public string? DepartmentName { get; set; }

        public string? Specialization { get; set; }

        public string? Contact { get; set; }

        public string? Status { get; set; } = "Active";

        

        [JsonIgnore]
        public int? DepartmentId { get; set; }

        [JsonIgnore]
        public Department? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
