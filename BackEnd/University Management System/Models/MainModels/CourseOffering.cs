namespace University_Management_System.Models.MainModels
{
    public class CourseOffering
    {
        public int Id { get; set; }

        
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // Which teacher is teaching the course (nullable if not yet assigned)
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        // In which semester the course is offered
        public int SemesterId { get; set; }
        public Semester Semester { get; set; }

        public string Status { get; set; } = "Active";
    }
}
