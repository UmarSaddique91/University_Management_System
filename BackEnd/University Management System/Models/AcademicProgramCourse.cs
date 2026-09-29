namespace University_Management_System.Models
{
    public class AcademicProgramCourse
    {
        public int Id { get; set; }

        public int AcademicProgramId { get; set; }
        public AcademicProgram? AcademicProgram { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }
    }
}


