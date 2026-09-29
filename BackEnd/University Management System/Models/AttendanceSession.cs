namespace University_Management_System.Models
{
    public class AttendanceSession
    {
        public int Id { get; set; }

        public int CourseOfferingId { get; set; }
        public CourseOffering CourseOffering { get; set; }

        public DateTime ClassDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}


