namespace University_Management_System.DTOs
{
    public class AddStudentDTO
    {
        public string UserId { get; set; }

        public string? StudentName { get; set; }
        public string Email { get; set; }

        public string? RollNumber { get; set; }

        public int? DepartmentId { get; set; }

        public int? AcademicProgramId { get; set; }

        public int? Semester { get; set; }

        public string Status { get; set; }

        
    }
}
