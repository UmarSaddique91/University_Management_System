namespace University_Management_System.Models.MainModels
{
    public class Student
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string StudentName { get; set; }

        public string RollNumber { get; set; }

        public int? DepartmentId { get; set; }

        public Department? Department { get; set; }

        public int? AcademicProgramId { get; set; }

        public AcademicProgram? AcademicProgram { get; set; }

        public int Semester { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
