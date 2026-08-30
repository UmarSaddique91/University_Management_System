namespace University_Management_System.Models.MainModels
{
    public class Assignment
    {
        public int Id { get; set; }
        public string? AssingmentCode { get; set; }
        public string? Department { get; set; }

        public int Credits { get; set; }

        public string? Semester { get; set; }
        public int Capacity { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public int AssignedTeacherId { get; set; }
        public Teacher AssignedTeacher { get; set; }

                 
    }
}
