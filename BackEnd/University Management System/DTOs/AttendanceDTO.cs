namespace University_Management_System.DTOs
{
    public class AttendanceDTO
    {
        public int StudentId { get; set; }
        public int CourseOfferingId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; }
    }
}
