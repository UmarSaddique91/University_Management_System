using University_Management_System.Models.MainModels;

namespace University_Management_System.DTOs
{
    public class AddTeacherDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }

        
        public string? TeacherId { get; set; }
        public string? TeacherName { get; set; }
       
        public string? Specialization { get; set; }
        public string? Contact { get; set; }
        public string? Status { get; set; }
        public int? DepartmentId { get; set; }

        
    }
}
