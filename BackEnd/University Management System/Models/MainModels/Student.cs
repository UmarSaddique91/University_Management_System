namespace University_Management_System.Models.MainModels
{
    public class Student
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public string? StudentName { get; set; }

        public string? RollNumber { get; set; }

        public int? DepartmentId { get; set; }

        public string? Email { get; set; }

        public Department? Department { get; set; }

        public int? AcademicProgramId { get; set; }

        public AcademicProgram? AcademicProgram { get; set; }

        public int? CurrentSemesterId { get; set; }

        public Semester? CurrentSemester { get; set; }

        public int? Semester { get; set; }

        public string? Status { get; set; } = "Inactive";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
