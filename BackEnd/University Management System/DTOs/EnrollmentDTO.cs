namespace University_Management_System.DTOs
{
    public class EnrollmentDTO
    {
        public int StudentId { get; set; }

        public int CourseOfferingId { get; set; }

        public DateTime EnrollmentDate { get; set; }

        public string Status { get; set; }
    }
}
