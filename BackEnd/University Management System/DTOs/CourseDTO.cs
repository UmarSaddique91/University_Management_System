namespace University_Management_System.DTOs
{
    public class CourseDTO
    {
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public int? CreditHours { get; set; }
        public string? Level { get; set; }
        public int? DepartmentId { get; set; }
        public string? Status { get; set; }
        public List<int>? AcademicProgramIds { get; set; }
    }

    public class UpdateCourseStatusDTO
    {
        public string? Status { get; set; }
    }
}
