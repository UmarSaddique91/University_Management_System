namespace University_Management_System.Models
{
    public class Exam
    {
        public int Id { get; set; }

        
        public int CourseOfferingId { get; set; }
        public CourseOffering CourseOffering { get; set; }

        
        public string ExamType { get; set; }

        
        public DateTime ExamDate { get; set; }

        
        public int TotalMarks { get; set; }
    }
}

