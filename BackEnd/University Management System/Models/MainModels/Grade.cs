namespace University_Management_System.Models.MainModels
{
    public class Grade
    {

        public int Id { get; set; }

        
        public int StudentId { get; set; }
        public Student Student { get; set; }

        
        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        
        public double ObtainedMarks { get; set; }

        
        public string GradeLetter { get; set; }
    }
}
