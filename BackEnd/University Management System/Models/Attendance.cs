namespace University_Management_System.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        
        //ForeignKeys
        public int StudentId { get; set; }
        public Student Student { get; set; }

        
        public int CourseOfferingId { get; set; }
        public CourseOffering CourseOffering { get; set; }

        
        public DateTime AttendanceDate { get; set; }

        public string Status { get; set; }

        public int? AttendanceSessionId { get; set; }
        public AttendanceSession? AttendanceSession { get; set; }
    }
}

