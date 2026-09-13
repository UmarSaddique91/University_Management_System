namespace University_Management_System.Models.MainModels
{
    public class Enrollment
    {

        public int Id { get; set; }

        
        public int StudentId { get; set; }
        public Student Student { get; set; }

        
        public int CourseOfferingId { get; set; }
        public CourseOffering CourseOffering { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public string Status { get; set; }

    }
}
