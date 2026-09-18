namespace University_Management_System.DTOs
{
    public class AddTeacherDTO
    {
        public int Id { get; set; }
        public string? TeacherId { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? TeacherName { get; set; }

        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public string? Specialization { get; set; }
        public string? Status { get; set; }
    }
}