namespace University_Management_System.DTOs
{
    public class CourseAssignmentDTO
    {
        public int CourseId { get; set; }

        public int? TeacherId { get; set; }

        public int SemesterId { get; set; }

        public string? Status { get; set; } = "Active";
    }
}

