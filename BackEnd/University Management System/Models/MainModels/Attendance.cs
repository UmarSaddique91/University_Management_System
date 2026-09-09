namespace University_Management_System.Models.MainModels
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
    }
}
