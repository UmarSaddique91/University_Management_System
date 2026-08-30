using Microsoft.AspNetCore.Mvc;

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

        public ICollection <Assignment>  AssignedCourses { get; set; } 

        public int DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}
