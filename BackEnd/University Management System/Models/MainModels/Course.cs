namespace University_Management_System.Models.MainModels
{
    public class Course
    {
        public int Id { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? DepartmentName { get; set; }
        public int CreditHours { get; set; }
        public string? Level { get; set; }
        public string? Status { get; set; }

        


        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        
    }
}
